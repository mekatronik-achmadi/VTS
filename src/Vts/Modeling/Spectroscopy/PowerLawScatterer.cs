using System;

namespace Vts.SpectralMapping
{
    /// <summary>
    /// Returns scattering values based on Steve Jacques' Skin Optics Summary:
    /// http://omlc.ogi.edu/news/jan98/skinoptics.html
    /// This returned reduced scattering follows the approximate formula:
    /// mus' = A1*lamda(-b1) + A2*lambda(-b2)
    /// </summary>
    public class PowerLawScatterer : BindableObject, IScatterer
    {
        /// <summary>
        /// Constructs a power law scatterer; i.e. mus' = a*lamda^-b + c*lambda^-d
        /// </summary>
        /// <param name="a">The first prefactor</param>
        /// <param name="b">The first exponent</param>
        /// <param name="c">The second prefactor</param>
        /// <param name="d">The second exponent</param>
        public PowerLawScatterer(double a, double b, double c, double d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        /// <summary>
        /// Creates a power law scatterer; i.e. mus' = a*lambda^-b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public PowerLawScatterer(double a, double b)
            : this(a,b,0.0,0.0)
        {
        }

        public PowerLawScatterer(TissueType tissueType)
        {
            SetTissueType(tissueType);
        }

        public PowerLawScatterer() 
            : this(TissueType.Custom)
        {
        }

        public void SetTissueType(TissueType tissueType)
        {
            switch (tissueType)
            {
                case (TissueType.Skin):
                    A = 1.2;
                    B = 1.42;
                    C = 0.0;
                    D = 0.0;
                    break;
                case TissueType.BreastPreMenopause:
                    A = 0.67;
                    B = 0.95;
                    C = 0.0;
                    D = 0.0;
                    break;
                case TissueType.BreastPostMenopause:
                    A = 0.72;
                    B = 0.58;
                    C = 0.0;
                    D = 0.0;
                    break;
                case (TissueType.BrainWhiteMatter):
                    A = 3.56;
                    B = 0.84;
                    C = 0.0;
                    D = 0.0;
                    break;
                case (TissueType.BrainGrayMatter):
                    A = 0.56;
                    B = 1.36;
                    C = 0.0;
                    D = 0.0;
                    break;
                case (TissueType.Liver):
                    A = 0.84;
                    B = 0.55;
                    C = 0.0;
                    D = 0.0;
                    break;
                case (TissueType.Custom):
                    A = 1;
                    B = 0.1;
                    C = 0.0;
                    D = 0.0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tissueType");
            }
        }

        public ScatteringType ScattererType { get { return ScatteringType.PowerLaw; } }

        public double A { get; set; }

        public double B { get; set; }

        public double C { get; set; }

        public double D { get; set; }

        /// <summary>
        /// Returns mus' based on Steve Jacques' Skin Optics Summary:
        /// http://omlc.ogi.edu/news/jan98/skinoptics.html
        /// </summary>
        /// <param name="wavelength"></param>
        /// <returns></returns>
        public double GetMusp(double wavelength)
        {
            return A * Math.Pow(wavelength/1000, - B) + C * Math.Pow(wavelength/1000, - D);
        }

        /// <summary>
        /// Returns a fixed g (scattering anisotropy) of 0.9
        /// </summary>
        /// <param name="wavelength">The wavelength, in nanometers</param>
        /// <returns>The scattering anisotropy. This is the cosine of the average scattering angle.</returns>
        public double GetG(double wavelength) { return 0.9; }

        /// <summary>
        /// Returns mus based on mus' and g 
        /// </summary>
        /// <param name="wavelength">The wavelength, in nanometers</param>
        /// <returns>The scattering coefficient, mus</returns>
        public double GetMus(double wavelength) { return GetMusp(wavelength) / (1.0 - GetG(wavelength)); }

    }
}
