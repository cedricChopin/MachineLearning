using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ESGI.Common;
using UnityEditor;

public class UseDllBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    public Points points;
    public FloatList outputExpected;
    public int[] npl  = new int[] { 2, 3, 4, 1 };
    private int nbInput;
    public List<double[]> dataset_inputs;
    public List<double[]> dataset_expected_outputs;
    public double[] resultBeforeTraining;
    public double[] resultAfterTraining;
    List<double[]> dataset_inputsCross;
    List<double[]> dataset_outputsCross;
    private GameObject spawn;
    private IntPtr modelPtr;
    void Start()
    {
        
        nbInput = points.positions.Count;
        nbInput = 500;
        dataset_inputsCross = inputCross();
        dataset_outputsCross = outputCross(dataset_inputsCross);
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
        dataset_inputs = dataset_inputsCross;
        dataset_expected_outputs = dataset_outputsCross;
        
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

            resultBeforeTraining = new double[nbInput];
            for (int i = 0; i < nbInput; i++)
            {
                var resultPtr = MyLibWrapper.predictMlpModelClassification(modelPtr, dataset_inputs[i], true);
                var result = new double[2];
                Marshal.Copy(resultPtr, result, 0, 2);
                resultBeforeTraining[i] = result[1];
            }
            MyLibWrapper.trainMlpModel(modelPtr, PtrSampleInputs, PtrExpectedOutputs, nbInput, nbInput, true, 0.01, 100000);
            
            resultAfterTraining = new double[nbInput];
            for (int i = 0; i < nbInput; i++)
            {
                var resultPtr = MyLibWrapper.predictMlpModelClassification(modelPtr, dataset_inputs[i], true);
                var result = new double[2];
                Marshal.Copy(resultPtr, result, 0, 2);
                resultAfterTraining[i] = result[1];
                //resultAfterTraining[i] = dataset_expected_outputs[i][0];
            }

            spawnSphereTest(); 
            showResult();


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
            pos[i].gameObject.GetComponent<Renderer>().material.color = resultAfterTraining[i - 1] < 0 ? Color.red : Color.blue;
        }
        
    }
}