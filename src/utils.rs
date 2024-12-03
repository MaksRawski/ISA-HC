use std::{f32::consts::PI, num::ParseIntError};

pub struct Precision {
    /// length of the binary representation of each possible solution within the SolutionSpace
    pub l: u32,
    /// smallest decimal step size e.g. 0.001
    pub d: f32,
}

// TODO: feels like not the best place for this method to exist but is convinient
//
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

        let l = ((b - a) / d + 1.0).log2().ceil() as u32;

        Ok(Self {
            a,
            b,
            precision: Precision { d, l },
        })
    }

    /// Creates a `SolutionSpace` using decimal places.
    pub fn from_decimal_places(a: f32, b: f32, decimal_places: u32) -> Result<Self, String> {
        if b < a {
            return Err(format!("Invalid range: [{}, {}]", a, b));
        }

        let step_size = 10_f32.powi(-(decimal_places as i32));
        let l = ((b - a) / step_size + 1.0).log2().ceil() as u32;

        Ok(Self {
            a,
            b,
            precision: Precision { d: step_size, l },
        })
    }

    /// Creates a `SolutionSpace` using bitstring length `l`.
    pub fn from_l(a: f32, b: f32, l: u32) -> Result<Self, String> {
        if l < 1 || l > 31 {
            return Err(format!(
                "Bitstring length must be in range [1, 31], got l={}",
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

pub fn f(x: f32) -> f32 {
    (x % 1.0) * ((20.0 * PI * x).cos() - x.sin())
}

pub fn f_bin(x: &Bin, space: &SolutionSpace) -> f32 {
    f(x.to_real(space).0)
}

pub struct Bin(pub u32);
pub struct Int(pub u32);
pub struct Real(pub f32);

impl Bin {
    pub fn to_int(&self) -> Int {
        Int(self.0)
    }
    pub fn to_real(&self, space: &SolutionSpace) -> Real {
        self.to_int().to_real(space)
    }
    pub fn flip_nth_bit(&self, n: u32) -> Bin {
        Bin(self.0 ^ (1 << n))
    }
}
impl Int {
    pub fn to_real(&self, space: &SolutionSpace) -> Real {
        Real(
            self.0 as f32 * (space.b - space.a) / (2_f32.powi(space.precision.l as i32) - 1.0)
                + space.a,
        )
    }
    pub fn to_bin(&self) -> Bin {
        Bin(self.0)
    }
}
impl Real {
    pub fn to_int(&self, space: &SolutionSpace) -> Int {
        Int(((self.0 - space.a) / (space.b - space.a)
            * (2_f32.powi(space.precision.l as i32) - 1.0))
            .round() as u32)
    }
    pub fn to_bin(&self, space: &SolutionSpace) -> Bin {
        self.to_int(space).to_bin()
    }
}
