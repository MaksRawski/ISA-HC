using System.Runtime.Intrinsics.X86;
using System.Transactions;

namespace Core;

/// <summary>
/// Population stores genotypes of all members as well as their values of goal function.
/// </summary>
public struct Population
{
    public List<double> xs, fs;
}
public readonly struct AlgorithmStats
{
    public readonly IReadOnlyList<IReadOnlyList<double>> fVbestsOfT;

    internal AlgorithmStats(List<List<double>> fVbestsOfT)
    {
        this.fVbestsOfT = fVbestsOfT;
    }
}
internal class StatsFactory
{
    private List<List<double>> fVbestsOfT;
    private List<double> fvCurrentBests;

    public StatsFactory(int T)
    {
        fVbestsOfT = new(T);
        fvCurrentBests = new();
    }

    public void AddfvBest(double fvBest)
    {
        fvCurrentBests.Add(fvBest);
    }
    public void NextT()
    {
        fVbestsOfT.Add(fvCurrentBests);
        fvCurrentBests = new();
    }
    public AlgorithmStats Build()
    {
        return new AlgorithmStats(fVbestsOfT);
    }
}

public class Algorithm
{
    private ThreadLocal<Random> _rand = new();
    private readonly UserInputs _inputs;
    public double fExtremeOppositeOfGoal { get; set; }

    public Algorithm(UserInputs userInputs)
    {
        _inputs = userInputs;
        _rand = new ThreadLocal<Random>(() => new Random());
        fExtremeOppositeOfGoal = _inputs.optimizationGoal == OptimizationGoal.Max ? double.MaxValue : double.MinValue;
    }
    public Algorithm(UserInputs userInputs, int seed)
    {
        _inputs = userInputs;
        _rand = new ThreadLocal<Random>(() => new Random(seed));
        fExtremeOppositeOfGoal = _inputs.optimizationGoal == OptimizationGoal.Max ? double.MaxValue : double.MinValue;
    }

    public void SetSeed(int seed)
    {
        _rand = new ThreadLocal<Random>(() => new Random(seed));
    }
    private string RandomBitString()
    {
        var randomInt = _rand.Value!.Next(0, (int)Math.Pow(2, _inputs.genotypeSpace.precision.l));
        return Utils.Int2Bin(randomInt, _inputs.genotypeSpace.precision.l);
    }
    private static string MutateNthBit(string xbin, int n)
    {
        var chars = xbin.ToCharArray();
        chars[n] = chars[n] == '1' ? '0' : '1';
        return new(chars);
    }

    /// <returns>best xbin after T generations</returns>
    public string Run(out AlgorithmStats stats)
    {
        var statsFactory = new StatsFactory(_inputs.T);
        var vbest = RandomBitString();
        var fvbest = _inputs.f(Utils.Bin2Real(vbest, _inputs.genotypeSpace));

        for (int i = 0; i < _inputs.T; ++i)
        {
            var vc = RandomBitString();
            var fvc = _inputs.f(Utils.Bin2Real(vc, _inputs.genotypeSpace));
            var localOptimum = false;

            while (!localOptimum)
            {
                statsFactory.AddfvBest(fvbest);
                localOptimum = true;
                for (int n = 0; n < _inputs.genotypeSpace.precision.l; n++)
                {
                    var vn = MutateNthBit(vc, n);
                    var vnReal = Utils.Bin2Real(vn, _inputs.genotypeSpace);
                    if (_inputs.f(vnReal) > fvbest)
                    {
                        vbest = vn;
                        fvbest = _inputs.f(Utils.Bin2Real(vbest, _inputs.genotypeSpace));
                        localOptimum = false;
                    }
                }
                vc = vbest;
            }
            statsFactory.NextT();
        }
        stats = statsFactory.Build();
        return vbest;
    }

    /// <summary>
    /// Rounds a real number with precision specified in an instance member '_inputs.genotypeSpace'.
    /// </summary>
    /// <param name="x">Number to round.</param>
    /// <returns>Rounded number.</returns>
    private double genotypeSpaceRound(double x)
    {
        return Math.Round(x, _inputs.genotypeSpace.precision.decimalPlaces);
    }
}
