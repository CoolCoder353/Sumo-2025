import Model
import csv
# Constants
debugMode = True

def GenerateRandomInput():
    import random
    # Generate a random input for the model
    input_data = [random.uniform(0, 1) for _ in range(5)]
    return input_data

# Load model
model = Model.Model('BoardCode\\SumoAgent.onnx')

# Open a CSV file to write predictions
with open('predictions.csv', mode='w', newline='') as file:
    writer = csv.writer(file)
    # Write the header row
    writer.writerow(['Iteration', 'Left Motor', 'Right Motor'])

    i = 0
    while i < 100:
        # Generate random input data
        input_data = GenerateRandomInput()
        
        # Run the model with the generated input data
        predictions = model.run(input_data, returnAllPredictions=False)[0]

        # Write predictions to the CSV file
        writer.writerow([i, predictions[0], predictions[1]])

        # Print predictions if debug mode is enabled
        if debugMode:
            print(f"Iteration {i}: Left Motor = {predictions[0]}, Right Motor = {predictions[1]}")
        
        i += 1