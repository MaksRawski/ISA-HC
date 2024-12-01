import javax.swing.*
import javax.swing.text.PlainDocument
import javax.swing.text.AttributeSet
import javax.swing.text.DocumentFilter

class AppWindow {
    private lateinit var frame: JFrame

    fun createAndShow() {
        frame = JFrame().apply {
            defaultCloseOperation = JFrame.EXIT_ON_CLOSE
            contentPane = InputPanel(::onValidInputs).panel
            pack()
            setLocationRelativeTo(null)
            isVisible = true
        }
    }

    private fun onValidInputs(inputs: Inputs) {
        JOptionPane.showMessageDialog(frame, "Inputs: $inputs")
    }
}

private class InputPanel(private val onValid: (Inputs) -> Unit) {
    val panel: JPanel = JPanel()
    private val aInput = createNumericField()
    private val bInput = createNumericField()
    private val dInput = JComboBox(arrayOf(0.1, 0.01, 0.001))
    private val TInput = createNumericField()
    private val startButton = JButton("START").apply { isEnabled = false }

    init {
        setupValidation()
        setupLayout()
        setupStartButton()
    }

    private fun setupLayout() {
        panel.apply {
            add(JLabel("a:")); add(aInput)
            add(JLabel("b:")); add(bInput)
            add(JLabel("d:")); add(dInput)
            add(JLabel("T:")); add(TInput)
            add(startButton)
        }
    }

    private fun setupValidation() {
        val validate = {
            startButton.isEnabled = validateInputs(aInput, bInput, dInput, TInput)
        }

        aInput.document.addDocumentListener(SimpleDocListener(validate))
        bInput.document.addDocumentListener(SimpleDocListener(validate))
        TInput.document.addDocumentListener(SimpleDocListener(validate))
        dInput.addActionListener { validate() }
    }

    private fun setupStartButton() {
        startButton.addActionListener {
            val inputs = Inputs(
                aInput.text.toDouble(),
                bInput.text.toDouble(),
                dInput.selectedItem as Double,
                TInput.text.toInt()
            )
            onValid(inputs)
        }
    }

    private fun validateInputs(
        aInput: JTextField,
        bInput: JTextField,
        dInput: JComboBox<Double>,
        TInput: JTextField
    ): Boolean = aInput.text.toDoubleOrNull() != null &&
            bInput.text.toDoubleOrNull() != null &&
            TInput.text.toIntOrNull() != null &&
            dInput.selectedItem != null &&
            aInput.text.toDouble() < bInput.text.toDouble()
}

private fun createNumericField(): JTextField = JTextField(10).apply {
    (document as PlainDocument).documentFilter = NumericFilter()
}

private class NumericFilter : DocumentFilter() {
    override fun insertString(fb: FilterBypass, offset: Int, string: String?, attr: AttributeSet?) {
        if (string?.all { it.isDigit() || it == '.' || it == '-' } == true) super.insertString(fb, offset, string, attr)
    }

    override fun replace(fb: FilterBypass, offset: Int, length: Int, string: String?, attrs: AttributeSet?) {
        if (string?.all { it.isDigit() || it == '.' || it == '-' } == true) super.replace(fb, offset, length, string, attrs)
    }
}

private class SimpleDocListener(private val callback: () -> Unit) : javax.swing.event.DocumentListener {
    override fun insertUpdate(e: javax.swing.event.DocumentEvent?) = callback()
    override fun removeUpdate(e: javax.swing.event.DocumentEvent?) = callback()
    override fun changedUpdate(e: javax.swing.event.DocumentEvent?) = callback()
}
