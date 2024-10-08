using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class EmailTemplateListingViewModel
    {
        public List<EmailTemplate> EmailTemplates { get; set; }
        public string SearchTerm { get; set; }
    }

    public class EmailTemplateActionViewModel
    {
        public List<VariableModel> Variables { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string TemplateCode { get; set; }
        public string Duration { get;  set; }

        public List<string> Durations { get; set; }
        public List<string> DurationsForFeedback { get;  set; }
    }


    public class VariableModel
    {
        public string VariableCode { get; set; }
        public string VariableDescription { get; set; }
    }
}