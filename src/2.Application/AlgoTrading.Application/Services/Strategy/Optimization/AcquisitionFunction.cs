namespace AlgoTrading.Application.Services.Strategy.Optimization;

/// <summary>
/// description(설명) : Acquisition Functions for Bayesian Optimization (베이지안 최적화를 위한 획득 함수)
/// Details(상세설명) : Implements various acquisition functions (EI, PI, UCB) to balance exploration vs exploitation
/// Applied technology patterns(적용기술패턴) : Bayesian Optimization, Decision Theory, Statistical Inference
/// </summary>
public static class AcquisitionFunction
{
    /// <summary>
    /// Expected Improvement (EI)
    /// Measures expected improvement over current best observation
    /// </summary>
    /// <param name="mean">Predicted mean from GP</param>
    /// <param name="stdDev">Predicted standard deviation from GP</param>
    /// <param name="bestObserved">Current best observed value</param>
    /// <param name="xi">Exploration-exploitation trade-off parameter (default: 0.01)</param>
    /// <returns>Expected improvement value</returns>
    public static double ExpectedImprovement(
        double mean,
        double stdDev,
        double bestObserved,
        double xi = 0.01)
    {
        if (stdDev < 1e-10)
            return 0.0;

        double improvement = mean - bestObserved - xi;
        double z = improvement / stdDev;

        // EI = improvement · Φ(z) + σ · φ(z)
        // where Φ is CDF and φ is PDF of standard normal distribution
        double ei = improvement * NormalCDF(z) + stdDev * NormalPDF(z);

        return Math.Max(ei, 0.0);
    }

    /// <summary>
    /// Probability of Improvement (PI)
    /// Probability that a point will improve over the current best
    /// </summary>
    /// <param name="mean">Predicted mean from GP</param>
    /// <param name="stdDev">Predicted standard deviation from GP</param>
    /// <param name="bestObserved">Current best observed value</param>
    /// <param name="xi">Exploration-exploitation trade-off parameter (default: 0.01)</param>
    /// <returns>Probability of improvement</returns>
    public static double ProbabilityOfImprovement(
        double mean,
        double stdDev,
        double bestObserved,
        double xi = 0.01)
    {
        if (stdDev < 1e-10)
            return 0.0;

        double z = (mean - bestObserved - xi) / stdDev;

        // PI = Φ(z)
        return NormalCDF(z);
    }

    /// <summary>
    /// Upper Confidence Bound (UCB)
    /// Balances mean prediction and uncertainty
    /// </summary>
    /// <param name="mean">Predicted mean from GP</param>
    /// <param name="stdDev">Predicted standard deviation from GP</param>
    /// <param name="kappa">Exploration parameter (default: 2.576 for 99% confidence)</param>
    /// <returns>UCB value</returns>
    public static double UpperConfidenceBound(
        double mean,
        double stdDev,
        double kappa = 2.576)
    {
        // UCB = μ + κ · σ
        return mean + kappa * stdDev;
    }

    /// <summary>
    /// Lower Confidence Bound (LCB) for minimization problems
    /// </summary>
    public static double LowerConfidenceBound(
        double mean,
        double stdDev,
        double kappa = 2.576)
    {
        // LCB = μ - κ · σ
        return mean - kappa * stdDev;
    }

    /// <summary>
    /// Thompson Sampling
    /// Samples from the posterior distribution
    /// </summary>
    public static double ThompsonSampling(
        double mean,
        double stdDev,
        Random random)
    {
        // Sample from N(μ, σ²)
        double u1 = random.NextDouble();
        double u2 = random.NextDouble();

        // Box-Muller transform
        double z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

        return mean + stdDev * z0;
    }

    /// <summary>
    /// Knowledge Gradient (simplified version)
    /// Estimates value of perfect information
    /// </summary>
    public static double KnowledgeGradient(
        double mean,
        double stdDev,
        double bestObserved,
        double currentBest)
    {
        if (stdDev < 1e-10)
            return 0.0;

        // Simplified KG: max(0, mean - currentBest) + σ
        double improvement = Math.Max(0, mean - currentBest);
        return improvement + stdDev;
    }

    /// <summary>
    /// Batch Expected Improvement for parallel evaluations
    /// Computes EI considering multiple points simultaneously
    /// </summary>
    public static List<double> BatchExpectedImprovement(
        List<(double mean, double stdDev)> predictions,
        double bestObserved,
        double xi = 0.01)
    {
        return predictions
            .Select(p => ExpectedImprovement(p.mean, p.stdDev, bestObserved, xi))
            .ToList();
    }

    #region Statistical Helper Functions

    /// <summary>
    /// Cumulative Distribution Function (CDF) of standard normal distribution
    /// </summary>
    private static double NormalCDF(double x)
    {
        // Using error function approximation
        return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
    }

    /// <summary>
    /// Probability Density Function (PDF) of standard normal distribution
    /// </summary>
    private static double NormalPDF(double x)
    {
        return Math.Exp(-0.5 * x * x) / Math.Sqrt(2.0 * Math.PI);
    }

    /// <summary>
    /// Error function approximation (Abramowitz and Stegun)
    /// </summary>
    private static double Erf(double x)
    {
        // Save the sign of x
        int sign = x >= 0 ? 1 : -1;
        x = Math.Abs(x);

        // Constants
        double a1 = 0.254829592;
        double a2 = -0.284496736;
        double a3 = 1.421413741;
        double a4 = -1.453152027;
        double a5 = 1.061405429;
        double p = 0.3275911;

        // A&S formula 7.1.26
        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

        return sign * y;
    }

    #endregion
}

/// <summary>
/// Acquisition function type enumeration
/// </summary>
public enum AcquisitionFunctionType
{
    /// <summary>
    /// Expected Improvement - balances exploration and exploitation
    /// </summary>
    ExpectedImprovement,

    /// <summary>
    /// Probability of Improvement - focuses on probability of beating current best
    /// </summary>
    ProbabilityOfImprovement,

    /// <summary>
    /// Upper Confidence Bound - optimistic exploration
    /// </summary>
    UpperConfidenceBound,

    /// <summary>
    /// Lower Confidence Bound - for minimization problems
    /// </summary>
    LowerConfidenceBound,

    /// <summary>
    /// Thompson Sampling - probabilistic sampling approach
    /// </summary>
    ThompsonSampling,

    /// <summary>
    /// Knowledge Gradient - value of perfect information
    /// </summary>
    KnowledgeGradient
}
