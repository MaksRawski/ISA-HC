use std::num::ParseIntError;

use crate::hill_climbing::SolutionSpace;

/// Converts a binary number to integer.
pub fn bin_to_int(binary_string: &str) -> Result<i32, ParseIntError> {
    i32::from_str_radix(binary_string, 2)
}

/// Converts an integer to a binary string of length `l`.
pub fn int_to_bin(x: i32, l: usize) -> String {
    format!("{:0l$b}", x, l = l)
}

/// Converts an integer to a real number within the solution space.
pub fn int_to_real(x: i32, space: &SolutionSpace) -> f32 {
    x as f32 * (space.b - space.a) / (2_f32.powi(space.precision.l as i32) - 1.0) + space.a
}

/// Converts a real number to an integer within the solution space.
pub fn real_to_int(x: f32, space: &SolutionSpace) -> i32 {
    ((x - space.a) / (space.b - space.a) * (2_f32.powi(space.precision.l as i32) - 1.0)).round()
        as i32
}

/// Converts a real number to a binary string within the solution space.
pub fn real_to_bin(x: f32, space: &SolutionSpace) -> String {
    let int_value = real_to_int(x, space);
    int_to_bin(int_value, space.precision.l)
}

/// Converts a binary string to a real number within the solution space.
pub fn bin_to_real(binary_string: &str, space: &SolutionSpace) -> Result<f32, ParseIntError> {
    let int_value = bin_to_int(binary_string)?;
    Ok(int_to_real(int_value, space))
}
