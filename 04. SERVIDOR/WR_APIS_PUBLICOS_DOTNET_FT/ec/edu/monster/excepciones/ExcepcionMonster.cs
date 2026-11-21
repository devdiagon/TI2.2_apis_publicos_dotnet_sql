namespace ec.edu.monster.excepciones;

public class ExcepcionMonster : Exception
{
    public ExcepcionMonster(string mensaje) : base(mensaje)
    {
    }

    public ExcepcionMonster(string mensaje, Exception causa) : base(mensaje, causa)
    {
    }
}
