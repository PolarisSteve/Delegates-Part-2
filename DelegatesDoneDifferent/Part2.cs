using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DelegatesDoneDifferent
{
    public partial class Part2 : Form
    {
        public Part2()
        {
            InitializeComponent();
        }

        //Each button is wrapped in it's own region which refers the buttons actions and supporting code. 

        #region Example One

        private void btnExample1_Click(object sender, EventArgs e)
        {
            //We are going to be testin integer values
            DelegateValidator<int> dint = new DelegateValidator<int>();

            bool retval;

            //Create a linked list of clsValidator, Current validator test to see if Value is greater than 1, 
            //if so then next validator (PassValidator) tests to see if Value is also < 10
            dint.ValidationRules = new clsValidator<int> { 
                CurrentValidator = (i) => { return i > 0; }, 
                PassValidator = new clsValidator<int> { 
                    CurrentValidator = (i) => { return i < 10; } 
                } 
            };

            dint.Value = -1;
            retval = dint.Valid(); //False, less then 0 - fails first validator, never goes to second validator

            dint.Value = 5;
            retval = dint.Valid(); //True, greater then 0 and less then 10 - both validators return true

            dint.Value = 50;
            retval = dint.Valid(); //False, greater then 0 and greater then 10 - first validator passes, second fails.


        }

        #endregion

        #region Example Two
        private void btnExample2_Click(object sender, EventArgs e)
        {
            //First test - defining validator with anonymous functions
            DelegateValidator<string> delstr = new DelegateValidator<string>();
            delstr.Value = "Steve";
            //inline validation - first length > 1 then first char is Upper case
            delstr.ValidationRules = new clsValidator<string>() 
                { CurrentValidator = (s) => { return s.Trim().Length > 1; }, 
                    PassValidator = new clsValidator<string>() 
                    { CurrentValidator = (s) => { return char.IsUpper(s, 0); } } 
                };
            bool isValid = delstr.Valid(); // With "Steve", Validation passes.

            //Second Test - defining validators with functions which match the signature.
            clsValidator<string> mv = new clsValidator<string>();
            mv.CurrentValidator = LengthMinValidator; //Check to see if it passes min length
            mv.PassValidator = new clsValidator<string>(); //If so,
            mv.PassValidator.CurrentValidator = LengthMaxValidator; //Check to see if max length

            mv.PassValidator.PassValidator = new clsValidator<string>() { 
                CurrentValidator = FirstLetterUpperValidator };
            
            mv.PassValidator.FailValidator = new clsValidator<string>();
            //If max validator fails, check to see if number.
            mv.PassValidator.FailValidator.CurrentValidator = IsNumberValidator; 


            delstr.ValidationRules = mv; //Assigning new rules.
            delstr.Value = "cat";
            isValid = delstr.Valid(); //Should be false, passes min and max but not Upper case;

            delstr.Value = "10000";
            isValid = delstr.Valid(); //Should be true, long and numeric

            delstr.Value = "10";
            isValid = delstr.Valid(); //Should be false, not long and not upper case;

            delstr.Value = "Cat";
            isValid = delstr.Valid(); //Should be true;

        }
        private bool LengthMinValidator(string s)
        {
            Console.WriteLine("LengthMinValidator");
            return s.Length > 1;
        }

        private bool LengthMaxValidator(string s)
        {
            Console.WriteLine("LengthMaxValidator");
            return s.Length < 5;
        }

        private bool FirstLetterUpperValidator(string s)
        {
            Console.WriteLine("FirstLetterUpperValidator");
            return Char.IsUpper(s, 0);
        }

        private bool IsNumberValidator(string s)
        {
            Console.WriteLine("IsNumberValidator");
            long j;
            return Int64.TryParse(s, out j);
        }

        #endregion

        #region Example Three
        private void btnExample3_Click(object sender, EventArgs e)
        {
            clsEmployee ce1 = new clsEmployee() { 
                FirstName = "Bob", LastName = "Marley" 
            };
            clsEmployee ce2 = new clsEmployee() { 
                FirstName = "Steve", 
                LastName = "Contos",
                Webpage = @"http://www.PolarisSolutions.com" 
            };
            //will fail on both webpage and if you fix page will fail on age
            clsEmployee ce3 = new clsEmployee() { 
                FirstName = "Bill", 
                LastName = "Gates", 
                Webpage = @"http://wwwMicrosoftcom",
                Age = 10, 
                BirthDate = Convert.ToDateTime("10/28/1955").Date 
            };

            clsEmployee ce4 = new clsEmployee()
            {
                FirstName = "Steve"
            };

            /*Test
                1. First Name has value
                2. Second Name has value
                3. If WebPage supplied, confirm valid
                4. If Age > 0 validate BirthDate
            */

            DelegateValidator<clsEmployee> dv = new DelegateValidator<clsEmployee>();
            dv.ValidationRules = new clsValidator<clsEmployee>()
            {
                CurrentValidator = checkFirstName,
                PassValidator = new clsValidator<clsEmployee>()
                {
                    //First name passes
                    CurrentValidator = checkLastName,
                    PassValidator = new clsValidator<clsEmployee>()
                    {
                        //Last name passes
                        CurrentValidator = checkWebAddress,
                        PassValidator = new clsValidator<clsEmployee>()
                        {
                            //Web address passes
                            CurrentValidator = checkAge
                        }
                    }
                }
            };

            bool retval = false;

            dv.Value = ce1;
            retval = dv.Valid(); //Should pass

            dv.Value = ce2;
            retval = dv.Valid(); //Should pass

            dv.Value = ce3;
            retval = dv.Valid(); //Should fail

            dv.Value = ce4;
            retval = dv.Valid(); //Should fail


        }

        private bool checkFirstName(clsEmployee e)
        {
            return e.FirstName != null && e.FirstName.Length > 0;
        }
        private bool checkLastName(clsEmployee e)
        {
            return e.LastName != null && e.LastName.Length > 0;
        }

        private bool checkWebAddress(clsEmployee e)
        {
            //Regex from google search :)    
            Regex rg = 
                new Regex(@"[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)");
            if (string.IsNullOrEmpty(e.Webpage))
                return true;
            if (e.Webpage.Trim().Length == 0)
                return true; // not an error
            else
                return rg.IsMatch(e.Webpage);
        }

        private bool checkAge(clsEmployee e)
        {
            if (e.Age > 0)
            {
                if (DateTime.Now.Year - e.BirthDate.Year == e.Age)
                    return true;

                return false;
            }
            else
                return true; //not an error not to enter an age
        
        }



        #endregion

    }
    #region Example Helper Classes
    class clsValidator<T>
    {
        public Func<T, bool> CurrentValidator { get; set; }
        public clsValidator<T> FailValidator { get; set; }
        public clsValidator<T> PassValidator { get; set; }

    }

    class DelegateValidator<T>
    {

        public T Value { get; set; }

        public clsValidator<T> ValidationRules { get; set; }

        internal bool Valid()
        {
            //Seed the recursive routine with root node
            return Valid(ValidationRules, Value);
        }
        /// <summary>
        /// This is a recursive call which traverses the clsValidator nodes.
        /// </summary>
        /// <param name="currval">Validator function</param>
        /// <param name="RecurseValue">Value</param>
        /// <returns>true or false</returns>
        private bool Valid(clsValidator<T> currval, T RecurseValue)
        {

            //invoke the Func<T,bool> to test the Value
            bool success = currval.CurrentValidator.Invoke(RecurseValue);
            if (success)
            {
                if (currval.PassValidator != null)
                    return Valid(currval.PassValidator, RecurseValue);
            }
            else
            {
                if (currval.FailValidator != null)
                    return Valid(currval.FailValidator, RecurseValue);
            }


            return success;


        }

    }

    class clsEmployee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public string Webpage { get; set; }

    
    }

    #endregion
}
