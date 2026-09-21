/// <summary>
/// Interface à implémenter sur les entités capables de recevoir des dégâts.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
}
