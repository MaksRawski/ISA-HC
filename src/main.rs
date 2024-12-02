mod hill_climbing;
mod utils;

use iced::{
    widget::{button, row, text_input, PickList, Row},
    Alignment,
};

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

struct State {
    inputs: Inputs,
    validated_inputs: Option<ValidatedInputs>,
}

impl Default for State {
    fn default() -> Self {
        Self {
            inputs: Default::default(),
            validated_inputs: Some(Default::default()),
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
    pub fn view(&self) -> Row<Message> {
        let d_input = row![
            "d =",
            PickList::new([0.1, 0.01, 0.001], Some(self.inputs.d), Message::UpdateD)
        ]
        .spacing(10)
        .align_y(Alignment::Center);

        let start_button = if self.validated_inputs.is_some() {
            button("START").on_press(Message::Start)
        } else {
            button("START")
        };

        // TODO: those should be number only inputs
        let create_input = |label, value, on_input: fn(String) -> Message| -> Row<Message> {
            row![label, text_input("", value).on_input(on_input)]
                .spacing(10)
                .align_y(Alignment::Center)
        };

        row![
            create_input("a =", &self.inputs.a, Message::UpdateA),
            create_input("b =", &self.inputs.b, Message::UpdateB),
            d_input,
            create_input("T =", &self.inputs.t, Message::UpdateT),
            start_button,
        ]
        .spacing(10)
    }

    pub fn update(&mut self, message: Message) {
        match message {
            Message::UpdateA(a) => self.inputs.a = a,
            Message::UpdateB(b) => self.inputs.b = b,
            Message::UpdateD(d) => self.inputs.d = d,
            Message::UpdateT(t) => self.inputs.t = t,
            Message::Start => todo!(
                "This message can only be sent when the button is enabled
and that can only happen if self.validated_inputs.is_some(). Figure out some logic for that!"
            ),
        }
        // TODO: input validation should probably only be done on input changes
        // but it looks better here :P
        self.validated_inputs = self.inputs.try_parse();
    }
}

fn main() -> iced::Result {
    iced::run("", State::update, State::view)
}
