using System;
using System.Runtime.InteropServices;

public static class MyLibWrapper
{
    [DllImport("MLP")]
    public static extern int return42();

    [DllImport("MLP")]
    public static extern IntPtr createMlpModel(int[] npl, int nplSize);

    [DllImport("MLP")]
    public static extern IntPtr forward_pass(IntPtr model, double[] sample_inputs, bool is_classification);

    [DllImport("MLP")]
    public static extern IntPtr trainMlpModel(IntPtr model, IntPtr[] samplesInputs,
        IntPtr[] samplesExpectedOutputs,
        int inputDim,
        int outputDim,
        bool is_classification,
        double alpha,
        int nbIter);

    [DllImport("MLP")]
    public static extern IntPtr predictMlpModelClassification(IntPtr model,
        double[] sampleInputs,
        bool isClassification);

    [DllImport("MLP")]
    public static extern void destroyMlpModel(IntPtr model);
    
    [DllImport("MLP")]
    public static extern void destroyMlpResult(IntPtr result);
}