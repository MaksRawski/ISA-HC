use std::f32::consts::PI;

use crate::ValidatedInputs;

pub struct Precision {
    /// length of the binary representation of each possible solution within the SolutionSpace
    pub l: usize,
    /// smallest decimal step size e.g. 0.001
    pub d: f32,
}

impl Precision {
    pub fn round(&self, x: f32) -> f32 {
        (x / self.d).round() * self.d
    }
}

pub struct SolutionSpace {
    pub a: f32,
    pub b: f32,
    pub precision: Precision,
}

impl SolutionSpace {
    pub fn from_d(a: f32, b: f32, d: f32) -> Result<Self, String> {
        if b < a {
            return Err(format!("Invalid range: [{}, {}]", a, b));
        }

        if d <= 0.0 || (d * 10.0_f32.powf(d.log10().floor())).fract() != 0.0 {
            return Err(format!(
                "Step size must be a fractional power of 10, but got d={}",
                d
            ));
        }

        let l = ((b - a) / d + 1.0).log2().ceil() as usize;

        Ok(Self {
            a,
            b,
            precision: Precision { d, l },
        })
    }

    /// Creates a `GenotypeSpace` using decimal places.
    pub fn from_decimal_places(a: f32, b: f32, decimal_places: usize) -> Result<Self, String> {
        if b < a {
            return Err(format!("Invalid range: [{}, {}]", a, b));
        }

        let step_size = 10_f32.powi(-(decimal_places as i32));
        let l = ((b - a) / step_size + 1.0).log2().ceil() as usize;

        Ok(Self {
            a,
            b,
            precision: Precision { d: step_size, l },
        })
    }

    /// Creates a `GenotypeSpace` using genotype length `l`.
    pub fn from_l(a: f32, b: f32, l: usize) -> Result<Self, String> {
        if l < 1 || l > 31 {
            return Err(format!(
                "Genotype length must be in range [1, 31], got l={}",
                l
            ));
        }

        let num_of_values = 2_usize.pow(l as u32);
        let step_size = (b - a) / (num_of_values as f32 - 1.0);

        if step_size > 1.0 {
            return Err(format!(
                "Range [{}, {}] cannot be represented with l={} bits.",
                a, b, l
            ));
        }

        Ok(Self {
            a,
            b,
            precision: Precision { d: step_size, l },
        })
    }
    pub fn a(&self) -> f32 {
        self.a
    }
}

fn f(x: f32) -> f32 {
    (x % 1.0) * ((20.0 * PI * x).cos() - x.sin())
}

pub fn hill_climb(inputs: &ValidatedInputs) -> f32 {
    let space = SolutionSpace::from_d(inputs.a, inputs.b, inputs.d);
    // let vc = random number that can fit in space.precision.l bits
    for t in 0..inputs.t {
        // create l variations of vc, each with nth bit mutated
        // and choose the best one
    }
    0.0
}
