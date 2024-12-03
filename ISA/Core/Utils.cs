﻿using org.mariuszgromada.math.mxparser;

namespace Core;

public struct UserInputs
{
    public GenotypeSpace genotypeSpace;
    public int T;
    public Func<double, double> f;
    public OptimizationGoal optimizationGoal;

    public UserInputs(GenotypeSpace genotypeSpace, int T, Func<double, double> f, OptimizationGoal functionGoal)
    {
        this.genotypeSpace = genotypeSpace;
        this.T = T;
        this.f = f;
        this.optimizationGoal = functionGoal;
    }
}

// the following may seem like quite a monster but it's only just a struct
// with 5 attributes where 3 of them are nested in another struct
// so it has tons of comments to explain why it's like that

/// <summary>
/// The space of all the possible genotypes described by an inclusive range 
/// [<see cref="a"/>, <see cref="b"/>] and a <see cref="Precision"/>.
/// </summary>
public readonly struct GenotypeSpace
{
    /// <summary>
    /// Lower boundary of the space range (inclusive).
    /// </summary>
    public readonly double a;
    /// <summary>
    /// Upper boundary of the space range (inclusive).
    /// </summary>
    public readonly double b;

    /// <summary>
    /// Defines how detailed a value can be through
    /// <see cref="d"/> - the smallest decimal step size,
    /// <see cref="l"/> - information capacity as a number of bits and 
    /// <see cref="decimalPlaces"/> - decimal precision of the number.
    /// </summary>
    public readonly struct Precision
    {
        /// <summary>
        /// Smallest decimal step size e.g. 0.001.
        /// </summary>
        public readonly double d;
        /// <summary>
        /// Length of the binary representation of the genotype.
        /// </summary>
        public readonly int l;
        /// <summary>
        /// Decimal places to be used for rounding the real number representation.
        /// </summary>
        public readonly int decimalPlaces;

        /// Internal constructor needed for the factory inside <see cref="GenotypeSpace"/> to be working.
        internal Precision(double d, int l, int decimalPlaces)
        {
            this.d = d;
            this.l = l;
            this.decimalPlaces = decimalPlaces;
        }
    }

    /// <summary>
    /// An instance member holding the <see cref="Precision"/> structure.
    /// </summary>
    public readonly Precision precision;

    /// Private constructor needed for the entire <see cref="GenotypeSpace"/> factory to be working.
    private GenotypeSpace(double a, double b, Precision precision)
    {
        if (b < a) throw new ArgumentException($"Tried to create a GenotypeSpace with an invalid range: [{a}, {b}]!");
        this.a = a;
        this.b = b;
        this.precision = precision;
    }

    /// <summary>
    /// Creates a <see cref="GenotypeSpace"/> instance using step size 'd'. 
    /// </summary>
    /// <param name="d">Step size e.g. 0.001.</param>
    /// <param name="a">The lower (inclusive) boundary of the range.</param>
    /// <param name="b">The upper (inclusive) boundary of the range.</param>
    /// <returns>A genotypeSpace instance with all the fields.</returns>
    public static GenotypeSpace FromD(double d, double a, double b)
    {
        if (d <= 0 || Math.Pow(10, Math.Log10(d)) != d)
        {
            throw new ArgumentException($"Step size must be a fractional power of 10, but given d={d}");
        }
        int decimalPlaces = (int)Math.Ceiling(-Math.Log10(d));
        int l = (int)Math.Ceiling(Math.Log2((b - a) / d + 1));
        Precision precision = new(d, l, decimalPlaces);

        return new GenotypeSpace(a, b, precision);
    }

    /// <summary>
    /// Creates a <see cref="GenotypeSpace"/> instance using 'decimalPlaces'. 
    /// </summary>
    /// <param name="decimalPlaces">Decimal places to be used for rounding the real number representation.</param>
    /// <param name="a">The lower (inclusive) boundary of the range.</param>
    /// <param name="b">The upper (inclusive) boundary of the range.</param>
    /// <returns>A genotypeSpace instance with all the fields.</returns>
    public static GenotypeSpace FromDecimalPlaces(int decimalPlaces, double a, double b)
    {
        try
        {
            checked
            {
                // number of solutions for given parameter set
                _ = (int)(b - a) * (int)Math.Pow(10, decimalPlaces) + 1;
            }
        }
        catch (OverflowException)
        {
            throw new ArgumentException(
                $"Too many solutions are possible for range [{a}, {b}] with decimal precision of {decimalPlaces}.");
        }

        double d = Math.Pow(10, -decimalPlaces);
        int l = (int)Math.Ceiling(Math.Log2((b - a) / d + 1));
        Precision precision = new(d, l, decimalPlaces);

        return new GenotypeSpace(a, b, precision);
    }

    /// <summary>
    /// Creates a <see cref="GenotypeSpace"/> instance using genotype length 'l'.
    /// </summary>
    /// <param name="l">Genotype length - number of bits of the binary representation.</param>
    /// <param name="a">The lower (inclusive) boundary of the range.</param>
    /// <param name="b">The upper (inclusive) boundary of the range.</param>
    /// <returns>A genotypeSpace instance with all the fields.</returns>
    public static GenotypeSpace FromL(int l, double a, double b)
    {
        // Biggest value that can be stored in a 32-bit signed int is 2^31-1 so a genotype
        // of length greater than 31 bits would allow for genotypes to be bigger than 2^31 which would
        // cause e.g. Bin2Int to overflow.
        if (l > 31 || l < 1)
        {
            throw new ArgumentException($"Genotype length must be in range [1, 31], asked for l={l}");
        }

        double numOfValues = Math.Pow(2, l);
        double d = (b - a) / (numOfValues - 1);
        if (d > 1)
        {
            throw new ArgumentException(
                $"Range [{a}, {b}] has length which can't be fit into a l={l} bit long number." +
                $"Trying to fit it would cause some integers to be impossible to represent." +
                $"Consider providing a bigger l.");
        }

        // nDecimalPlaces is the exponent in the scientific notation, rounded up to
        // always choose coarser precision, ensuring that every number can be represented
        // with the later calculated step size d.
        int nDecimalPlaces = (int)Math.Ceiling(Math.Log10(d));
        d = Math.Pow(10, nDecimalPlaces);
        int decimalPlaces = -nDecimalPlaces;

        Precision precision = new(d, l, decimalPlaces);

        return new GenotypeSpace(a, b, precision);
    }
}

public enum OptimizationGoal
{
    Max,
    Min,
}
public class TableRow
{
    public int Lp { get; set; }
    public double XReal { get; set; }
    public string? XBin { get; set; }
    public double Fx { get; set; }
    public double Percent { get; set; }

}

public class Utils
{
    public static int Bin2Int(string binaryString)
    {
        return Convert.ToInt32(binaryString, 2);
    }
    public static string Int2Bin(int x, int l)
    {
        return Convert.ToString(x, 2).PadLeft(l, '0');
    }

    public static double Int2Real(int x, GenotypeSpace space)
    {
        return x * (space.b - space.a) / (Math.Pow(2, space.precision.l) - 1) + space.a;
    }
    public static int Real2Int(double x, GenotypeSpace space)
    {
        return (int)Math.Round((x - space.a) / (space.b - space.a) * (Math.Pow(2, space.precision.l) - 1));
    }
    /// <summary>
    /// Converts a Real representation of a genotype to Binary within the <see cref="GenotypeSpace"/>.
    /// </summary>
    /// <param name="x">Real number representation of the genotype.</param>
    /// <param name="space">A <see cref="GenotypeSpace"/> within which to change the representation.</param>
    /// <returns>A real number representation of the genotype.</returns>
    public static string Real2Bin(double x, GenotypeSpace space)
    {
        return Int2Bin(Real2Int(x, space), space.precision.l);
    }
    /// <summary>
    /// Converts a Binary representation of a genotype to a Real number within the <see cref="GenotypeSpace"/>.
    /// </summary>
    /// <param name="x">Binary representation of the genotype.</param>
    /// <param name="space">A <see cref="GenotypeSpace"/> within which to change the representation.</param>
    /// <returns>A real number representation of the genotype.</returns>
    public static double Bin2Real(string x, GenotypeSpace space)
    {
        return Int2Real(Bin2Int(x), space);
    }
    private static double Gmax(Func<double, double> f, double x, double fMin, double d)
    {
        return f(x) - fMin + d;
    }
    private static double Gmin(Func<double, double> f, double x, double fMax, double d)
    {
        return -(f(x) - fMax) + d;
    }
    public static double G(Func<double, double> f, double x, OptimizationGoal functionGoal, double fExtreme, double d)
    {
        return functionGoal == OptimizationGoal.Max ?
            Gmax(f, x, fExtreme, d) : Gmin(f, x, fExtreme, d);
    }
    /// <summary>
    /// Finds the index of the smallest element in the sorted list 'qs' that is greater than or equal to the given value 'r' using Binary Search.
    /// </summary>
    /// <param name="r">The threshold value to compare against the elements in 'qs'.</param>
    /// <param name="qs">A sorted list of double values representing cumulative distribution function (CDF).</param>
    /// <returns>The index of the first element in 'qs' that is greater than or equal to 'r'.</returns>
    public static int GetCDFIndex(double r, List<double> qs)
    {
        int low = 0, high = qs.Count - 1;

        while (low < high)
        {
            int mid = (low + high) / 2;

            if (qs[mid] < r)
                low = mid + 1;
            else
                high = mid;
        }

        return low;
    }
    public static bool TryParseFunction(string expression, out Func<double, double>? f)
    {
        Argument xArg = new("x");
        Expression e = new(expression);
        e.addArguments(xArg);
        bool isFunctionValid = e.checkSyntax();

        f = null;
        if (isFunctionValid)
        {
            f = (x) =>
            {
                e.setArgumentValue("x", x);
                return e.calculate();
            };
        }

        return isFunctionValid;
    }
    public static Func<double, double> ParseFunction(string expression)
    {
        Argument xArg = new("x");
        Expression e = new(expression);
        e.addArguments(xArg);

        return (x) =>
        {
            e.setArgumentValue("x", x);
            return e.calculate();
        };
    }
}