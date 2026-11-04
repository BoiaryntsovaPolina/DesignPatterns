namespace LightCalculatorLab.Interfaces;

// Абстракція вводу/виводу: логіка програми не залежить від Console
public interface IUserInterface
{
    void ShowMessage(string message);
    void ShowError(string message);

    // Повертає null, якщо введення закінчилось (Ctrl+Z)
    string? ReadLine(string prompt);
}
