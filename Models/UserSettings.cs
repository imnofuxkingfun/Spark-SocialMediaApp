using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum ContentFilterType
{
    Gore,
    Nudity,
    Suggestive,
    Sensitive
}



namespace Spark_SocialMediaApp.Models
{


    public class UserSettings
    {
        public static Dictionary<string, bool> ContentFilterInit(bool value=true)
        {
            
            Dictionary<string, bool> temp = new Dictionary<string, bool>();
            foreach (ContentFilterType filter in Enum.GetValues(typeof(ContentFilterType)).Cast<ContentFilterType>())
            {
                temp.Add(filter.ToString(), value);
            }
            return temp;
        }

        [Key]
        public string UserId { get; set; } //user id

        private bool privacyPublic = true; //public -> accepts all following requests
                                //private -> manually accepts following requests

        private string base_color = "dark";
        private string accent_color = "purple";


         private Dictionary<string, bool> contentFilters = ContentFilterInit();

        //get set
        public bool PrivacyPublic
        {
            get { return privacyPublic; }
            set { privacyPublic = value; }
        }

        public string BaseColor
        {
            get { return base_color; }
            set { base_color = value; }
        }

        public string AccentColor
        {
            get { return accent_color; }
            set { accent_color = value; }
        }

        public Dictionary<string, bool> ContentFilters
        {
            get { return contentFilters; }
            set { contentFilters = value; }
        }



        public virtual User? User { get; set; }
    }
}
