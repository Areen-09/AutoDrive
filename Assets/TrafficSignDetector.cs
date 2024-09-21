using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;

public class TrafficSignDetector : MonoBehaviour
{
    public NNModel modelAsset;
    public RawImage cameraFeed;
    private Model model;
    private IWorker worker;

    void Start()
    {
        // Load the model
        model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
    }

    void Update()
    {
        // Get camera image
        Texture2D cameraImage = GetCameraImage();

        // Preprocess the image
        Tensor input = TransformInput(cameraImage);

        // Run the model
        worker.Execute(input);
        Tensor output = worker.PeekOutput();

        // Get the detected class (for traffic sign detection)
        int predictedClass = ArgMax(output.ToReadOnlyArray());

        // Take actions based on detected traffic sign
        ProcessTrafficSign(predictedClass);

        // Clean up
        input.Dispose();
        output.Dispose();
    }

    Texture2D GetCameraImage()
    {
        // Capture the camera feed to a Texture2D
        Texture2D texture = new Texture2D(cameraFeed.texture.width, cameraFeed.texture.height, TextureFormat.RGB24, false);
        texture.SetPixels(((Texture2D)cameraFeed.texture).GetPixels());
        texture.Apply();
        return texture;
    }

    Tensor TransformInput(Texture2D image)
    {
        // Resize and normalize the image
        float[] floatValues = new float[3 * 64 * 64];
        for (int i = 0; i < 64 * 64; i++)
        {
            Color pixel = image.GetPixel(i % 64, i / 64);
            floatValues[i * 3 + 0] = pixel.r;
            floatValues[i * 3 + 1] = pixel.g;
            floatValues[i * 3 + 2] = pixel.b;
        }

        return new Tensor(1, 3, 64, 64, floatValues);
    }

    int ArgMax(float[] array)
    {
        int maxIndex = 0;
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] > array[maxIndex])
            {
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    void ProcessTrafficSign(int detectedClass)
    {
        // Implement actions based on the detected traffic sign
        switch (detectedClass)
        {
            case 0: // Stop Sign
                Debug.Log("Stop Sign Detected");
                // Implement car stopping behavior
                break;
            // Add more cases for other traffic signs
        }
    }

    void OnDestroy()
    {
        worker.Dispose();
    }
}
