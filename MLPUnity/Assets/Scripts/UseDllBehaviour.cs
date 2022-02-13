using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ESGI.Common;
using UnityEditor;
using System.IO;

public class UseDllBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    public Points points;
    public FloatList outputExpected;
    public int[] npl  = new int[] { 2, 3, 4, 3 };
    private int nbInput;
    public List<double[]> dataset_inputs;
    public List<double[]> dataset_expected_outputs;
    public List<double[]> resultBeforeTraining;
    public List<double[]> resultAfterTraining;
    List<double[]> dataset_inputsCross;
    List<double[]> dataset_outputsCross;

    List<double[]> dataset_inputsImage;
    List<double[]> dataset_outputsImage;
    private GameObject spawn;
    private IntPtr modelPtr;
    private RetrieveFiles retrieveFiles;
    void Start()
    {
        dataset_inputsImage = new List<double[]>();
        dataset_outputsImage = new List<double[]>();
        retrieveFiles = GameObject.Find("Files").GetComponent<RetrieveFiles>();
        var dictionaryFiles = RetrieveFiles.GetDictionaryGenresFiles("GenresBis");
        int indexDir = 0;
        foreach (KeyValuePair<string, List<List<double>>> keyValue in dictionaryFiles)
        {
            string key = keyValue.Key;
            List<List<double>> value = keyValue.Value;
            foreach (var file in value)
            {
                //var pixels = RetrieveFiles.getPixelsFromImage(file);
                var pixels = file;
                dataset_inputsImage.Add(pixels.ToArray());
                var output = new double[4] { -1, -1, -1, -1 };
                output[indexDir] = 1;
                dataset_outputsImage.Add(output);

            }
            indexDir++;
        }

        nbInput = dataset_inputsImage.Count;
        dataset_inputsCross = inputCross();
        dataset_outputsCross = outputLinear3Classes(dataset_inputsCross);
        if (points.positions.Count != outputExpected.Output.Count)
        {
            EditorApplication.isPlaying = false;
        }
        modelPtr = MyLibWrapper.createMlpModel(npl, npl.Length);

        dataset_inputs = new List<double[]>();
        foreach(Vector2 point in points.positions)
        {
            dataset_inputs.Add(new double[] { point.x, point.y });
        }
        
        dataset_expected_outputs = new List<double[]>();
        foreach(float resExpected in outputExpected.Output)
        {
            dataset_expected_outputs.Add(new double[] { resExpected });
        }
        dataset_inputs = dataset_inputsImage;
        dataset_expected_outputs = dataset_outputsImage;
        
        try
        {
            IntPtr[] PtrSampleInputs = new IntPtr[nbInput];
            IntPtr[] PtrExpectedOutputs = new IntPtr[nbInput];
            int sizeInput = Marshal.SizeOf(dataset_inputs[0][0]) * dataset_inputs[0].Length;
            int sizeOutput = Marshal.SizeOf(dataset_expected_outputs[0][0]) * dataset_expected_outputs[0].Length;
            for (int i = 0; i < nbInput; i++)
            {
                PtrSampleInputs[i] = Marshal.AllocHGlobal(sizeInput);
                PtrExpectedOutputs[i] = Marshal.AllocHGlobal(sizeOutput);
                Marshal.Copy(dataset_inputs[i], 0, PtrSampleInputs[i], dataset_inputs[i].Length);
                Marshal.Copy(dataset_expected_outputs[i], 0, PtrExpectedOutputs[i], dataset_expected_outputs[i].Length);
            }

            resultBeforeTraining = new List<double[]>();
            for (int i = 0; i < nbInput; i++)
            {
                var resultPtr = MyLibWrapper.predictMlpModelClassification(modelPtr, dataset_inputs[i], true);
                var result = new double[npl[npl.Length - 1] + 1];
                Marshal.Copy(resultPtr, result, 0, npl[npl.Length - 1] + 1);
                result = result.Skip(1).ToArray();
                resultBeforeTraining.Add(result);
            }
            MyLibWrapper.trainMlpModel(modelPtr, PtrSampleInputs, PtrExpectedOutputs, nbInput, nbInput, true, 0.01, 10000);
            
            resultAfterTraining = new List<double[]>();
            for (int i = 0; i < nbInput; i++)
            {
                var resultPtr = MyLibWrapper.predictMlpModelClassification(modelPtr, dataset_inputs[i], true);
                var result = new double[npl[npl.Length - 1] + 1];
                Marshal.Copy(resultPtr, result, 0, npl[npl.Length - 1] + 1);
                result = result.Skip(1).ToArray();
                resultAfterTraining.Add(result);
                //resultAfterTraining.Add(dataset_expected_outputs[i]);
            }

            spawnSphereTest(); 
            //showResult();


            //MyLibWrapper.destroyMlpModel(modelPtr);
            //MyLibWrapper.destroyMlpResult(resultPtr);
        }
        catch
        {
            Debug.Log("error");
        }
    }


    private void Update()
    {
        /*for (int i = 0; i < nbInput; i++)
        {
            //dataset_inputs[i] = new double[] { points.positions[i].x, points.positions[i].y };
            var resultPtr = MyLibWrapper.predictMlpModelClassification(modelPtr, dataset_inputs[i], true);
            var result = new double[3];
            Marshal.Copy(resultPtr, result, 0, 3);
            resultAfterTraining[i] = result[1];
        }*/
        showResult();
    }

    private List<double[]> inputCross()
    {
        List<double[]> dataset_inputsCross = new List<double[]>();

        for (int i = 0; i < nbInput; i++)
        {
            double rand = UnityEngine.Random.Range(-100, 100);
            rand = rand / 100.0;
            double rand_2 = UnityEngine.Random.Range(-100, 100);
            rand_2 = rand_2 / 100.0;
            dataset_inputsCross.Add(new double[] { rand, rand_2 });
        }

        return dataset_inputsCross;
    }

    private List<double[]> outputCross(List<double[]> inputs)
    {
        List<double[]> dataset_outputsCross = new List<double[]>();

        for (int i = 0; i < nbInput; i++)
        {
            if(Math.Abs(inputs[i][0]) <= 0.3 || Math.Abs(inputs[i][1]) <= 0.3)
            {
                dataset_outputsCross.Add(new double[] { 1 });
            }
            else
            {
                dataset_outputsCross.Add(new double[] { -1 });

            }
            
        }

        return dataset_outputsCross;
    }


    private List<double[]> outputLinear3Classes(List<double[]> inputs)
    {
        List<double[]> dataset_outputsCross = new List<double[]>();

        for (int i = 0; i < nbInput; i++)
        {
            if ((-inputs[i][0] - inputs[i][1]  - 0.5 > 0) && (inputs[i][1] < 0) && (inputs[i][0] - inputs[i][1] - 0.5 < 0)) 
            {
                dataset_outputsCross.Add(new double[] { 1, 0, 0 });
            }
            else if ((-inputs[i][0] - inputs[i][1] - 0.5 < 0) && (inputs[i][1] > 0) && (inputs[i][0] - inputs[i][1] - 0.5 < 0))
            {
                dataset_outputsCross.Add(new double[] { 0, 1, 0 });

            }
            else if ((-inputs[i][0] - inputs[i][1] - 0.5 < 0) && (inputs[i][1] < 0) && (inputs[i][0] - inputs[i][1] - 0.5 > 0))
            {
                dataset_outputsCross.Add(new double[] { 0, 0, 1 });

            }
            else
            {
                dataset_outputsCross.Add(new double[] { 0, 0, 0 });

            }

        }

        return dataset_outputsCross;
    }
    private void spawnSphereTest()
    {
        spawn = new GameObject();
        spawn.name = "Tests";
        foreach(double[] point in dataset_inputs)
        {
            Vector2 p = new Vector2((float)point[0], (float)point[1]);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale /= 30;
            sphere.transform.position = new Vector3(p.x, 0, p.y);
            sphere.name = "Sphere pos " + p.x + " " + p.y;
            sphere.transform.parent = spawn.transform;
        }
    }

    private void showResult()
    {

        Transform[] pos = spawn.GetComponentsInChildren<Transform>();
        for (int i = 1; i < pos.Length; ++i)
        {
            //pos[i].gameObject.GetComponent<Renderer>().material.color = resultAfterTraining[i - 1] < 0 ? Color.red : Color.blue;
            float r = 0;
            float g = 0;
            float b = 0;
            if (resultAfterTraining[i - 1][0] > resultAfterTraining[i - 1][1] && resultAfterTraining[i - 1][0] > resultAfterTraining[i - 1][2])
            {
                r = 1;
            }
            else if (resultAfterTraining[i - 1][1] > resultAfterTraining[i - 1][0] && resultAfterTraining[i - 1][1] > resultAfterTraining[i - 1][2])
            {
                g = 1;
            }
            else if (resultAfterTraining[i - 1][2] > resultAfterTraining[i - 1][0] && resultAfterTraining[i - 1][2] > resultAfterTraining[i - 1][1])
            {
                b = 1;
            }
            pos[i].gameObject.GetComponent<Renderer>().material.color = new Color(r, g, b) ;
        }
        
    }
}