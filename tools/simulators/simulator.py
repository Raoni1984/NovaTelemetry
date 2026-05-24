import time
import requests
import random
from datetime import datetime, timezone

API_URL = "http://localhost:5000/api/v1/telemetry/batch"

def generate_telemetry_batch(batch_size=10, state="NORMAL"):
    batch = []
    
    for _ in range(batch_size):
        # 1. Fine-tuning sensors based on machinery state
        if state == "ANOMALY":
            # Simulates critical overheating and erratic behavior
            temperature = round(random.uniform(210.0, 250.0), 2)
            vibration = round(random.uniform(7.5, 12.0), 2)
            power_output = round(random.uniform(60.0, 75.0), 2)  # Loss of efficiency
        else:
            # Default stable operational behavior
            temperature = round(random.uniform(175.0, 185.0), 2)
            vibration = round(random.uniform(1.2, 3.5), 2)
            power_output = round(random.uniform(94.0, 99.0), 2)

        telemetry = {
            "equipmentId": "laser-fentum-01",
            "timestamp": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
            "temperature": temperature,
            "vibration": vibration,
            "powerOutput": power_output
        }

        batch.append(telemetry)
        time.sleep(0.1) # Small interval between internal readings
        
    return batch

def main():
    print("Intelligent Telemetry Simulator Started...")
    print("Press Ctrl+C to terminate at any time.\n")
    
    cycle_counter = 0
    current_state = "NORMAL"

    while True:
        try:
            # State alternator: every 5 normal batches, generates 1 anomaly batch
            cycle_counter += 1
            if cycle_counter % 6 == 0:
                current_state = "ANOMALY"
                print(f"⚠️ [ALERT] Injecting anomalous behavior into the system...")
            else:
                current_state = "NORMAL"

            # Generates and sends the data batch
            batch = generate_telemetry_batch(batch_size=5, state=current_state)
            
            response = requests.post(API_URL, json=batch, timeout=5)
            
            if response.status_code in [200, 201]:
                print(f"[{datetime.now().strftime('%H:%M:%S')}] Batch ({current_state}) successfully sent! Status: {response.status_code}")
            else:
                print(f"❌ Ingestion failed. Server responded with status: {response.status_code}")
                print(f"Error: {response.text}")

            # Quota control: Waits 3 seconds before sending the next complete batch
            time.sleep(3.0)

        except requests.exceptions.ConnectionError:
            print("❌ Connection Error: Is the .NET API running on the correct port?")
            time.sleep(5)
        except KeyboardInterrupt:
            print("\n🛑 Simulator safely terminated by the user.")
            break

if __name__ == "__main__":
    main()