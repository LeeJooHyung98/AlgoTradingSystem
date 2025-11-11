using System.Numerics;

namespace AlgoTrading.Application.Services.Strategy.Optimization;

/// <summary>
/// description(설명) : Gaussian Process for Bayesian Optimization (베이지안 최적화를 위한 가우시안 프로세스)
/// Details(상세설명) : Implements Gaussian Process Regression with RBF kernel for surrogate model in Bayesian Optimization
/// Applied technology patterns(적용기술패턴) : Machine Learning, Bayesian Inference, Kernel Methods
/// </summary>
public class GaussianProcess
{
    private readonly double _lengthScale;
    private readonly double _variance;
    private readonly double _noise;

    // Training data
    private List<double[]> _xTrain;
    private List<double> _yTrain;

    // Kernel matrix and its inverse
    private double[,]? _kernelMatrix;
    private double[,]? _kernelInverse;

    /// <summary>
    /// Initialize Gaussian Process with hyperparameters
    /// </summary>
    /// <param name="lengthScale">Length scale for RBF kernel (controls smoothness)</param>
    /// <param name="variance">Signal variance</param>
    /// <param name="noise">Observation noise</param>
    public GaussianProcess(double lengthScale = 1.0, double variance = 1.0, double noise = 1e-6)
    {
        _lengthScale = lengthScale;
        _variance = variance;
        _noise = noise;

        _xTrain = new List<double[]>();
        _yTrain = new List<double>();
    }

    /// <summary>
    /// Fit the Gaussian Process to training data
    /// </summary>
    public void Fit(List<double[]> xTrain, List<double> yTrain)
    {
        if (xTrain.Count != yTrain.Count)
            throw new ArgumentException("X and Y must have the same length");

        if (xTrain.Count == 0)
            throw new ArgumentException("Training data cannot be empty");

        _xTrain = xTrain.Select(x => x.ToArray()).ToList();
        _yTrain = new List<double>(yTrain);

        // Compute kernel matrix
        int n = _xTrain.Count;
        _kernelMatrix = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                _kernelMatrix[i, j] = RBFKernel(_xTrain[i], _xTrain[j]);

                // Add noise to diagonal
                if (i == j)
                {
                    _kernelMatrix[i, j] += _noise;
                }
            }
        }

        // Compute inverse using Cholesky decomposition
        _kernelInverse = CholeskyInverse(_kernelMatrix);
    }

    /// <summary>
    /// Predict mean and standard deviation for new points
    /// </summary>
    public (double mean, double stdDev) Predict(double[] xNew)
    {
        if (_xTrain.Count == 0)
            throw new InvalidOperationException("Model must be fitted before prediction");

        // Compute kernel vector between new point and training points
        int n = _xTrain.Count;
        double[] kStar = new double[n];

        for (int i = 0; i < n; i++)
        {
            kStar[i] = RBFKernel(xNew, _xTrain[i]);
        }

        // Compute mean: k* · K^(-1) · y
        double mean = 0.0;
        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < n; j++)
            {
                sum += _kernelInverse![i, j] * _yTrain[j];
            }
            mean += kStar[i] * sum;
        }

        // Compute variance: k** - k* · K^(-1) · k*^T
        double kStarStar = RBFKernel(xNew, xNew);
        double variance = kStarStar;

        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < n; j++)
            {
                sum += _kernelInverse![j, i] * kStar[j];
            }
            variance -= kStar[i] * sum;
        }

        // Ensure variance is non-negative
        variance = Math.Max(variance, 1e-10);

        double stdDev = Math.Sqrt(variance);

        return (mean, stdDev);
    }

    /// <summary>
    /// Predict mean and standard deviation for multiple points (optimized batch prediction)
    /// </summary>
    public List<(double mean, double stdDev)> PredictBatch(List<double[]> xNewList)
    {
        var results = new List<(double mean, double stdDev)>();

        // Parallel processing for better performance
        var predictions = new (double mean, double stdDev)[xNewList.Count];

        Parallel.For(0, xNewList.Count, i =>
        {
            predictions[i] = Predict(xNewList[i]);
        });

        results.AddRange(predictions);
        return results;
    }

    /// <summary>
    /// RBF (Radial Basis Function) Kernel
    /// k(x, x') = σ² · exp(-||x - x'||² / (2 · l²))
    /// </summary>
    private double RBFKernel(double[] x1, double[] x2)
    {
        if (x1.Length != x2.Length)
            throw new ArgumentException("Vectors must have the same length");

        double squaredDistance = 0.0;
        for (int i = 0; i < x1.Length; i++)
        {
            double diff = x1[i] - x2[i];
            squaredDistance += diff * diff;
        }

        return _variance * Math.Exp(-squaredDistance / (2.0 * _lengthScale * _lengthScale));
    }

    /// <summary>
    /// Compute matrix inverse using Cholesky decomposition
    /// More numerically stable and efficient for positive definite matrices
    /// </summary>
    private double[,] CholeskyInverse(double[,] matrix)
    {
        int n = matrix.GetLength(0);

        // Cholesky decomposition: K = L · L^T
        double[,] L = CholeskyDecomposition(matrix);

        // Solve L · Y = I
        double[,] Y = new double[n, n];
        for (int j = 0; j < n; j++)
        {
            Y[j, j] = 1.0;
            for (int i = j + 1; i < n; i++)
            {
                double sum = 0.0;
                for (int k = j; k < i; k++)
                {
                    sum += L[i, k] * Y[k, j];
                }
                Y[i, j] = -sum / L[i, i];
            }
        }

        // Solve L^T · X = Y
        double[,] inverse = new double[n, n];
        for (int j = 0; j < n; j++)
        {
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = Y[i, j];
                for (int k = i + 1; k < n; k++)
                {
                    sum -= L[k, i] * inverse[k, j];
                }
                inverse[i, j] = sum / L[i, i];
            }
        }

        return inverse;
    }

    /// <summary>
    /// Cholesky decomposition: A = L · L^T
    /// </summary>
    private double[,] CholeskyDecomposition(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        double[,] L = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                double sum = 0.0;

                if (j == i)
                {
                    for (int k = 0; k < j; k++)
                    {
                        sum += L[j, k] * L[j, k];
                    }
                    L[j, j] = Math.Sqrt(Math.Max(matrix[j, j] - sum, 1e-10));
                }
                else
                {
                    for (int k = 0; k < j; k++)
                    {
                        sum += L[i, k] * L[j, k];
                    }
                    L[i, j] = (matrix[i, j] - sum) / L[j, j];
                }
            }
        }

        return L;
    }

    /// <summary>
    /// Get number of training samples
    /// </summary>
    public int SampleCount => _xTrain.Count;

    /// <summary>
    /// Clear training data (memory optimization)
    /// </summary>
    public void Clear()
    {
        _xTrain.Clear();
        _yTrain.Clear();
        _kernelMatrix = null;
        _kernelInverse = null;
    }
}
