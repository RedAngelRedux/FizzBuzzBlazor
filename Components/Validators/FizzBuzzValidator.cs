using FizzBuzzBlazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Threading.Tasks;

namespace FizzBuzzBlazor.Components.Validators;

public class FizzBuzzValidator : ComponentBase, IDisposable
{
    private ValidationMessageStore? validatedMessageStore;

    private EventHandler<FieldChangedEventArgs>? fieldChangedHandler;
    private EventHandler<ValidationRequestedEventArgs>? validationRequestedHandler;

    [CascadingParameter]
    private EditContext CurrentEditContext { get; set; } = default!;

    [Parameter]
    public EventCallback OnValidationError { get; set; }

    protected override void OnInitialized()
    {
        if(CurrentEditContext == null)
        {
            throw new InvalidOperationException($"{nameof(FizzBuzzValidator)} requires a cascading parameter of type {nameof(EditContext)}.  For example, you can use {nameof(FizzBuzzValidator)} inside and {nameof(EditForm)}.");
        }

        validatedMessageStore = new ValidationMessageStore(CurrentEditContext);

        // Capture handlers so we can unsubscribe later
        fieldChangedHandler = (s, e) => ValidateField(e.FieldIdentifier);
        validationRequestedHandler = (s, e) => ValidateAllFieldss();

        CurrentEditContext.OnFieldChanged += fieldChangedHandler;
        CurrentEditContext.OnValidationRequested += validationRequestedHandler;       
    }

    private async void ValidateField(FieldIdentifier fieldIdentifier)
    {
        // Get the model from the EditContext
        var fizzbuzz = CurrentEditContext.Model as FizzBuzzModel ?? throw new InvalidOperationException($"{nameof(FizzBuzzValidator)} requires a model of type {nameof(FizzBuzzModel)}");

        // Clear previous validation messages for the field
        if(validatedMessageStore == null)
        {
            throw new InvalidOperationException($"{nameof(FizzBuzzValidator)} requires a {nameof(ValidationMessageStore)} to store validation messages.");
        }
        else
        {
            validatedMessageStore.Clear(fieldIdentifier);            
        }            

        // Validate the field based on its identifier
        if(fieldIdentifier.FieldName == nameof(FizzBuzzModel.FizzValue))
        {
            if (fizzbuzz.FizzValue >= fizzbuzz.BuzzValue)
            {
                validatedMessageStore.Add(fieldIdentifier, "The Fizz value must be less than the Buzz value.");                
            }
        }
        else if (fieldIdentifier.FieldName == nameof(FizzBuzzModel.BuzzValue))
        {
            if (fizzbuzz.BuzzValue <= fizzbuzz.FizzValue)
            {
                validatedMessageStore.Add(fieldIdentifier, "The Buzz value must be greater than the Fizz value.");
            }
        }
        else  if (fieldIdentifier.FieldName == nameof(FizzBuzzModel.StopValue))
        {
            var minValue = fizzbuzz.FizzValue * fizzbuzz.BuzzValue;
            if (fizzbuzz.StopValue < minValue)
            {
                validatedMessageStore.Add(fieldIdentifier, $"The Stop value must be at least {minValue}.");
            }
        }

        if(CurrentEditContext.GetValidationMessages().Any())
        {
            // If there are validation errors, trigger the OnValidationError callback
            //await OnValidationError.InvokeAsync();
            if (OnValidationError.HasDelegate)
            {
                await OnValidationError.InvokeAsync();
            }

        }
    }

    private void ValidateAllFieldss()
    {
        // Get the model from the EditContext
        var fizzbuzz = CurrentEditContext.Model as FizzBuzzModel ?? throw new InvalidOperationException($"{nameof(FizzBuzzValidator)} requires a model of type {nameof(FizzBuzzModel)}");     
       
        // Validate all fields
        ValidateField(new FieldIdentifier(fizzbuzz, nameof(FizzBuzzModel.FizzValue)));
        ValidateField(new FieldIdentifier(fizzbuzz, nameof(FizzBuzzModel.BuzzValue)));
        ValidateField(new FieldIdentifier(fizzbuzz, nameof(FizzBuzzModel.StopValue)));
    }

    public void Dispose()
    {
        if (CurrentEditContext != null)
        {
            if (fieldChangedHandler != null)
                CurrentEditContext.OnFieldChanged -= fieldChangedHandler;

            if (validationRequestedHandler != null)
                CurrentEditContext.OnValidationRequested -= validationRequestedHandler;
        }
    }
}
