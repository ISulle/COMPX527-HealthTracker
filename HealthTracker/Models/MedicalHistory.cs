using Amazon.DynamoDBv2.DataModel;

namespace HealthTracker.Models;

public class MedicalHistory
{
    [DynamoDBProperty("enteredMedicalHistory")]
    public bool EnteredMedicalHistory { get; set; }

    [DynamoDBProperty("polyuria")]
    public bool Polyuria { get; set; }

    [DynamoDBProperty("polydipsia")]
    public bool Polydipsia { get; set; }

    [DynamoDBProperty("suddenWeightLoss")]
    public bool SuddenWeightLoss { get; set; }

    [DynamoDBProperty("weakness")]
    public bool Weakness { get; set; }

    [DynamoDBProperty("polyphagia")]
    public bool Polyphagia { get; set; }

    [DynamoDBProperty("genitalThrush")]
    public bool GenitalThrush { get; set; }

    [DynamoDBProperty("visualBlurring")]
    public bool VisualBlurring { get; set; }

    [DynamoDBProperty("itching")]
    public bool Itching { get; set; }

    [DynamoDBProperty("irritability")]
    public bool Irritability { get; set; }

    [DynamoDBProperty("delayedHealing")]
    public bool DelayedHealing { get; set; }

    [DynamoDBProperty("partialParesis")]
    public bool PartialParesis { get; set; }

    [DynamoDBProperty("muscleStiffness")]
    public bool MuscleStiffness { get; set; }

    [DynamoDBProperty("alopecia")]
    public bool Alopecia { get; set; }

    [DynamoDBProperty("obesity")]
    public bool Obesity { get; set; }

    public MedicalHistory()
    {
        EnteredMedicalHistory = false;
    }
}
