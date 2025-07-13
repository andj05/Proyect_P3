

using System.Collections.Generic;

namespace Proyect_P3.Models.ViewModels
{
    public class Motores
    {
        public int IdMotor { get; set; }

        public string Descripcion { get; set; }

        public string Combustible { get; set; }

        public string Transmisión { get; set; }
    

    // Propiedades calculadas para mostrar información completa
        public string DescripcionCompleta
        {
            get
            {
                var partes = new List<string>();

                if (!string.IsNullOrEmpty(Descripcion))
                    partes.Add(Descripcion);

                if (!string.IsNullOrEmpty(Combustible))
                    partes.Add(Combustible);

                if (!string.IsNullOrEmpty(Transmisión))
                    partes.Add(Transmisión);

                return string.Join(" - ", partes);
            }
        }

        // Para mostrar solo combustible y transmisión
        public string MotorInfo
        {
            get
            {
                var info = new List<string>();

                if (!string.IsNullOrEmpty(Combustible))
                    info.Add(Combustible);

                if (!string.IsNullOrEmpty(Transmisión))
                    info.Add(Transmisión);

                return string.Join(" / ", info);
            }
        } 
    }
}