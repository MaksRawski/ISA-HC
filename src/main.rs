mod hill_climbing;
mod utils;

use hill_climbing::hill_climb;
use iced::{
    widget::{button, column, row, text_input, PickList, Row},
    Alignment, Element,
};
use utils::{f, Bin, Real, SolutionSpace};

#[derive(Clone, Copy)]
struct ValidatedInputs {
    a: f32,
    b: f32,
    d: f32,
    t: u32,
}

impl Default for ValidatedInputs {
    fn default() -> Self {
        Self {
            a: -4.0,
            b: 12.0,
            d: 0.001,
            t: 50,
        }
    }
}

struct Inputs {
    a: String,
    b: String,
    d: f32,
    t: String,
}

impl Inputs {
    pub fn try_parse(&self) -> Option<ValidatedInputs> {
        Some(ValidatedInputs {
            a: self.a.parse().ok()?,
            b: self.b.parse().ok()?,
            d: self.d,
            t: self.t.parse().ok()?,
        })
    }
}

impl Default for Inputs {
    fn default() -> Self {
        let validated_inputs = ValidatedInputs::default();
        Self {
            a: validated_inputs.a.to_string(),
            b: validated_inputs.b.to_string(),
            d: validated_inputs.d,
            t: validated_inputs.t.to_string(),
        }
    }
}

struct Results(Real, Bin, f32);

struct State {
    inputs: Inputs,
    validated_inputs: Option<ValidatedInputs>,
    solution_space: Option<SolutionSpace>,
    results: Option<Results>,
}

impl Default for State {
    fn default() -> Self {
        Self {
            inputs: Default::default(),
            validated_inputs: Some(Default::default()),
            solution_space: None,
            results: None,
        }
    }
}

#[derive(Debug, Clone)]
pub enum Message {
    UpdateA(String),
    UpdateB(String),
    UpdateD(f32),
    UpdateT(String),
    Start,
}

impl State {
    pub fn view(&self) -> Element<Message> {
        let d_input = row![
            "d =",
            PickList::new([0.1, 0.01, 0.001], Some(self.inputs.d), Message::UpdateD)
        ]
        .spacing(10)
        .align_y(Alignment::Center);

        let mut start_button = button("START");
        if self.validated_inputs.is_some() {
            start_button = start_button.on_press(Message::Start)
        }

        // TODO: those should be number only inputs
        let create_input = |label, value, on_input: fn(String) -> Message| -> Row<Message> {
            row![label, text_input("", value).on_input(on_input)]
                .spacing(10)
                .align_y(Alignment::Center)
        };

        let controls = row![
            create_input("a =", &self.inputs.a, Message::UpdateA),
            create_input("b =", &self.inputs.b, Message::UpdateB),
            d_input,
            create_input("T =", &self.inputs.t, Message::UpdateT),
            start_button,
        ]
        .spacing(10);

        if let Some(results) = &self.results {
            let results_header = row!["x real", "x bin", "f(x)"];
            let Results(xreal, xbin, fx) = results;

            // TODO: 3 should actually be decimal_places
            let xreal = format!("{:.*}", 3, xreal.0);
            let xbin = format!("{:.*}", 3, xbin.0);
            let fx = format!("{:.*}", 3, fx);

            let results_values: Row<(&str, &str, &str)> =
                row![xreal.as_str(), xbin.as_str(), fx.as_str()];
            let results_table = column![results_header, results_values];

            column![controls, results_table].into()
        } else {
            controls.into()
        }
    }

    pub fn update(&mut self, message: Message) {
        match message {
            Message::UpdateA(a) => self.inputs.a = a,
            Message::UpdateB(b) => self.inputs.b = b,
            Message::UpdateD(d) => self.inputs.d = d,
            Message::UpdateT(t) => self.inputs.t = t,
            Message::Start => {
                // TODO: instead of unwrapping display error
                let validated_inputs = self.validated_inputs.unwrap().clone();

                self.solution_space = SolutionSpace::from_d(
                    validated_inputs.a,
                    validated_inputs.b,
                    validated_inputs.d,
                )
                .ok();

                // TODO: again handle it

                if let Some(solution_space) = &self.solution_space {
                    let xreal = hill_climb(&solution_space, validated_inputs.t as usize);
                    let xbin = xreal.to_bin(&solution_space);
                    let fx = f(xreal.0);
                    self.results = Some(Results(xreal, xbin, fx));
                }
            }
        }
        // TODO: input validation should probably only be done on input changes
        // but it looks better here :P
        self.validated_inputs = self.inputs.try_parse();
    }
}

fn main() -> iced::Result {
    iced::run("", State::update, State::view)
}
