// ConsoleApplication1.cpp : Ce fichier contient la fonction 'main'. L'exécution du programme commence et se termine à cet endroit.
//

#include <iostream>
#include "../Dll1/pch.cpp"
int main()
{
    auto npl = new int[] { 2, 3, 4, 1 };
    double** dataset_inputs = new double* [4];
    dataset_inputs[0] = new double[] {0, 0};
    dataset_inputs[1] = new double[] {1, 0};
    dataset_inputs[2] = new double[] {0, 1};
    dataset_inputs[3] = new double[] {1, 1};

    double** dataset_expected_outputs = new double* [4];
    dataset_expected_outputs[0] = new double[] {-1};
    dataset_expected_outputs[1] = new double[] {1};
    dataset_expected_outputs[2] = new double[] {1};
    dataset_expected_outputs[3] = new double[] {-1};

    auto model = createMlpModel(npl, 4);
    std::cout << "Prediction : " << std::endl;
    for (int i = 0; i < 4; i++) {
        double* res = predictMlpModelClassification(model, dataset_inputs[i]);
        std::cout << res[1] << std::endl;
    }
    std::cout << "Training..." << std::endl;
    trainMlpModel(model, dataset_inputs, dataset_expected_outputs, 4, 4, true, 0.01, 100000);

    std::cout << "Prediction after training : " << std::endl;
    for (int i = 0; i < 4; i++) {
        double* res = predictMlpModelClassification(model, dataset_inputs[i]);
        std::cout << res[1] << std::endl;
    }
    
    
}

