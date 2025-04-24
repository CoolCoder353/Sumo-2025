pathToModel = "BoardCode\SumoAgent.onnx"
import onnxruntime as ort
import numpy as np

# Placeholder for sensor output (to be replaced with actual sensor data)
sensorOutput = {"obs_0": 0.5, "obs_1": 0.3, "obs_2": 0.2, "obs_3": 0.1, "obs_4": 0.4}
# Load the ONNX model
ort_sess = ort.InferenceSession(pathToModel)

# Run inference
outputs = ort_sess.run(None, {'input': sensorOutput})

result = outputs[0].argmax(axis=1)+1
print(result[0])