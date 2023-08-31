namespace HealthTracker.Data;

public class DiabetesInput
{
    public int Age { get; set; }
    public int Gender { get; set; }
    public bool Polyuria { get; set; }
    public bool Polydipsia { get; set; }
    public bool SuddenWeightLoss { get; set; }
    public bool Weakness { get; set; }
    public bool Polyphagia { get; set; }
    public bool GenitalThrush { get; set; }
    public bool VisualBlurring { get; set; }
    public bool Itching { get; set; }
    public bool Irritability { get; set; }
    public bool DelayedHealing { get; set; }
    public bool PartialParesis { get; set; }
    public bool MuscleStiffness { get; set; }
    public bool Alopecia { get; set; }
    public bool Obesity { get; set; }
    public bool Class { get; set; }

    public DiabetesInput()
    {
    }
}

public class DiabetesPrediction
{
    public bool PredictedLabel { get; set; }
    public float Score { get; set; }

    public DiabetesPrediction()
    {
    }
}
