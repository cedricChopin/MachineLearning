// pch.cpp : fichier source correspondant à l'en-tête précompilé

#include "pch.h"
#include <vector>
#include <assert.h>
#include <math.h>

struct MyMLP {
    
    double*** W;
    int* npl;
    double** X;
    double** deltas;
    int L;
};


extern "C" {
    __declspec(dllexport) int return42() {
        return 51;
    }


    __declspec(dllexport) MyMLP* createMlpModel(int npl[], int nplSize) {
        auto model = new MyMLP();

        double *** W = new double**[nplSize];
        for (int l = 0; l < nplSize; ++l) {
            
            if (l == 0) {
                W[l] = new double* [0];
                continue;
            }
           
            W[l] = new double*[npl[l - 1] + 1];
            for (int i = 0; i < npl[l - 1] + 1; ++i) {
                W[l][i] = new double[npl[l] + 1];
                for (int j = 0; j < npl[l] + 1; ++j) {
                    if (j != 0) {
                        double random = ((double)rand() / (RAND_MAX));
                        random = random * 2.0 - 1.0;
                        W[l][i][j] = random;
                    }
                    else
                        W[l][i][j] = (double)0.0;
                }
            }
        }
        double** X = new double* [nplSize];
        for (int l = 0; l < nplSize; ++l) {
            X[l] = new double[npl[l] + 1];
            for (int j = 0; j < npl[l] + 1; ++j) {
                if (j == 0)
                    X[l][j] = (double)1.0;
                else
                    X[l][j] = (double)0.0;
            }
        }

        double** deltas = new double* [nplSize];
        for (int l = 0; l < nplSize; ++l) {
            deltas[l] = new double[npl[l] + 1];
            for (int j = 0; j < npl[l] + 1; ++j) {
                deltas[l][j] = (double)0.0;
            }
        }
        model->W = W;
        model->npl = npl;
        model->X = X;
        model->deltas = deltas;
        model->L = nplSize - 1;
        return model;
    }
    __declspec(dllexport) void forward_pass(MyMLP* model, double sample_inputs[], bool is_classification) {

        for (int j = 1; j < model->npl[0] + 1; ++j) {
            model->X[0][j] = sample_inputs[j - 1];

        }
        for (int l = 1; l < model->L + 1; ++l) {
            for (int j = 1; j < model->npl[l] + 1; ++j) {
                double total = 0.0;
                for (int i = 0; i < model->npl[l - 1] + 1; ++i) {
                    total += model->W[l][i][j] * model->X[l - 1][i];
                }
                if (l != model->L || is_classification) {
                    total = tanh(total);
                }
                model->X[l][j] = total;
            }
        }
    }


    __declspec(dllexport) void trainMlpModel(MyMLP* model,
        double* samplesInputs[],
        double* samplesExpectedOutputs[],
        int inputDim,
        int outputDim,
        bool is_classification,
        double alpha,
        int nbIter

    ) {
        for (int it = 0; it < nbIter; ++it) {
            int k = rand() % inputDim;
            double* sample_inputs = samplesInputs[k];
            double* sample_expected_output = samplesExpectedOutputs[k];
            forward_pass(model, sample_inputs, is_classification);
            for (int j = 1; j < model->npl[model->L] + 1; ++j) {
                model->deltas[model->L][j] = (model->X[model->L][j] - sample_expected_output[j - 1]);
                if (is_classification) {
                    model->deltas[model->L][j] *= (1 - (model->X[model->L][j] * model->X[model->L][j]));
                }
            }
            for (int l = model->L; l > 2; --l) {
                for (int i = 1; i < model->npl[l - 1] + 1; ++i) {
                    double total = 0.0;
                    for (int j = 1; j < model->npl[l] + 1; ++j) {
                        total += model->W[l][i][j] * model->deltas[l][j];
                    }
                    total *= (1 - pow(model->X[l - 1][i], 2));
                    model->deltas[l - 1][i] = total;
                    
                }
            }
            for (int l = 1; l < model->L + 1; ++l) {
                for (int i = 0; i < model->npl[l - 1] + 1; ++i) {
                    for (int j = 1; j < model->npl[l] + 1; ++j) {
                        model->W[l][i][j] -= alpha * model->X[l - 1][i] * model->deltas[l][j];
                    }
                }
            }

        }
    }

    

    __declspec(dllexport) double* predictMlpModelClassification(MyMLP* model,
        double sampleInputs[],
        bool isClassification) 
    {
        forward_pass(model, sampleInputs, isClassification);
        return model->X[model->L];
    }

    __declspec(dllexport) void destroyMlpModel(MyMLP* model) {
        delete model;
    }

    __declspec(dllexport) void destroyMlpResult(const double* result) {
        delete[]result;
    }

}
