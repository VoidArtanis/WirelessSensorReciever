namespace ImagioDevice
{
    public class ThermalResistanceSensor : Sensor
    {

        public enum ThermalSensors
        {
            Sensor1,
            Sensor2
        }

        public ThermalResistanceSensor(ThermalSensors functionIndex) : base((int)functionIndex)
        {
        }

        public ThermalResistanceSensor(ThermalSensors functionIndex, long readinterval) : base((int)functionIndex, readinterval)
        {
        }
    }
}