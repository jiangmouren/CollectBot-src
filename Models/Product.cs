using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
#pragma warning disable 649

// The SandwichOrder is the simple form you want to fill out.  It must be serializable so the bot can be stateless.
// The order of fields defines the default order in which questions will be asked.
// Enumerations shows the legal options for each field in the SandwichOrder and the order is the order values will be presented 
// in a conversation.
namespace Microsoft.Bot.Sample.FormBot
{
    public enum ProductCategories
    {
        [Description("Auto And Tires")]
        AutoAndTires,
        [Description("Baby")]
        Baby,
        [Description("Books")]
        Books,
        [Description("Cell Phones")]
        CellPhones,
        [Description("Clothing")]
        Clothing,
        [Description("Electronics")]
        Electronics,
        [Description("Home Improvement")]
        HomeImprovement,
        [Description("Jewelry")]
        Jewelry,
        [Description("Office")]
        Office,
        [Description("Pets")]
        Pets,
        [Description("Pharmacy")]
        Pharmacy,
        [Description("Sports And Outdoors")]
        SportsAndOutdoors,
    };

    [Serializable]
    public class SandwichOrder
    {
        [Prompt("What kind of {&} would you like? {||}")]
        public ProductCategories? Products;

        public static IForm<SandwichOrder> BuildForm()
        {
            OnCompletionAsyncDelegate<SandwichOrder> processOrder = async (context, state) =>
            {
                await context.PostAsync("This is the end of the form, you would give a final confirmation, and then start the ordering process as needed.");
            };

            return new FormBuilder<SandwichOrder>()
                    .Message("Welcome to online grocery store!")
                    //eljian: By passing the state info, the engine can reflect all fields. No need to pass in per field info. 
                    //eljian: But by passing in field info, like the following one, "Cheese" will be prompt first.
                    //.Field(nameof(Cheese))
                    .OnCompletion(processOrder)
                    .Build();
        }
    };
}