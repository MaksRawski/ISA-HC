import javax.swing.UIManager
import javax.swing.JButton
import javax.swing.JFrame
import javax.swing.JPanel

fun setNativeLookAndFeel() {
	try {
		UIManager.setLookAndFeel(
			UIManager.getCrossPlatformLookAndFeelClassName())
	} catch (e: Exception) {
		e.printStackTrace()
	}
}

fun main() {
	setNativeLookAndFeel()

	val frame = JFrame("Minimal Swing Application")
	frame.defaultCloseOperation = JFrame.EXIT_ON_CLOSE
	frame.setSize(300, 200)

	// Create a JPanel and add components
	val panel = JPanel()
	val button = JButton("Click Me!")
	button.addActionListener {
		println("Button clicked!")
	}

	panel.add(button)
	frame.contentPane.add(panel)

	// Display the window
	frame.isVisible = true
}
