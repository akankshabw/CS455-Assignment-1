namespace MoleProject.Shared
{
    public class CellModel
    {
        public bool isShown { get; set; }

        public int Id { get; set; }
        public string Style { get; set; } = "";
        public bool IsShown
        {
            get => isShown;
            set
            {
                isShown = value;
                if (isShown)
                {
                    Style = "mole";
                }
                else
                {
                    Style = "";
                }
            }
        }
    }
}