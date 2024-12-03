use crate::utils::{f_bin, Bin, Real, SolutionSpace};
use rand::{thread_rng, Rng};

fn random_l_bit_bin<R: Rng>(rng: &mut R, l: u32) -> Bin {
    Bin(rng.next_u32() & (2_u32.pow(l) - 1))
}

pub fn hill_climb(space: &SolutionSpace, t: usize) -> Real {
    let mut rng = thread_rng();

    let mut vc = random_l_bit_bin(&mut rng, space.precision.l as u32);

    // let vc = random number that can fit in space.precision.l bits
    for _ in 0..t {
        loop {
            let mutations = (0..space.precision.l).map(|n| vc.flip_nth_bit(n));
            let best_mutation = mutations
                .max_by(|x, y| {
                    f_bin(x, &space)
                        .partial_cmp(&f_bin(y, &space))
                        .expect("NaN appeared in mutations!")
                })
                .unwrap();

            if f_bin(&best_mutation, &space) > f_bin(&vc, &space) {
                vc = best_mutation;
            } else {
                break;
            }
        }
    }
    vc.to_real(&space)
}
