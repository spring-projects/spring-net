#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using Spring.Expressions;
using Spring.Util;

namespace Spring.Validation.Validators
{
    /// <summary>
    /// Perform credit card validations.
    /// </summary>
    /// <remarks>
    /// By default, all supported card types are allowed. You can specify
    /// which credit card type validator should be used by setting
    /// the value of <see cref="CardType"/> property to a concrete <see cref="ICreditCardType"/>
    /// instance.
    /// </remarks>
    public class CreditCardValidator : BaseSimpleValidator
    {
        #region Properties

        /// <summary>
        /// Credit card type validator to use.
        /// </summary>
        /// <remarks>
        /// Can be concrete implementations of <see cref="ICreditCardType"/>
        /// interface. The following are available implementations:
        /// <see cref="Visa"/>, <see cref="Mastercard"/>, <see cref="Amex"/>,
        /// <see cref="Discover"/>.
        /// </remarks>
        public ICreditCardType CardType
        {
            get { return m_cardType; }
            set { m_cardType = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <b>UrlValidator</b> class.
        /// </summary>
        public CreditCardValidator()
        {}

        /// <summary>
        /// Creates a new instance of the <b>UrlValidator</b> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        /// <param name="cardType">Credit Card type validator to use.</param>
        public CreditCardValidator(string test, string when, ICreditCardType cardType)
            : base(test, when)
        {
            AssertUtils.ArgumentHasText(test, "test");
            this.m_cardType = cardType;
        }

        /// <summary>
        /// Creates a new instance of the <b>UrlValidator</b> class.
        /// </summary>
        /// <param name="test">The expression to validate.</param>
        /// <param name="when">The expression that determines if this validator should be evaluated.</param>
        /// <param name="cardType">Credit Card type validator to use.</param>
        public CreditCardValidator(IExpression test, IExpression when, ICreditCardType cardType)
            : base(test, when)
        {
            AssertUtils.ArgumentNotNull(test, "test");
            this.m_cardType = cardType;
        }

        #endregion

        #region BaseValidator methods

        /// <summary>
        /// Validates the supplied <paramref name="objectToValidate"/>.
        /// </summary>
        /// <remarks>
        /// In the case of the <see cref="CreditCardValidator"/> class,
        /// the test should be a string variable that will be evaluated and the object
        /// obtained as a result of this evaluation will be checked if it is
        /// a valid credit card number.
        /// </remarks>
        /// <param name="objectToValidate">The object to validate.</param>
        /// <returns>
        /// <see lang="true"/> if the supplied <paramref name="objectToValidate"/> is valid
        /// credit card number.
        /// </returns>
        protected override bool Validate(object objectToValidate)
        {
            string text = objectToValidate as string;
            if (StringUtils.IsNullOrEmpty(text))
            {
                return true;
            }

            return IsValid(text);
        }

        #endregion

        #region CreditCardValidator methods

        /// <summary>
        /// Checks if the <paramref name="card"/> is a valid credit card number.
        /// </summary>
        /// <param name="card">
        /// The card number to validate.
        /// </param>
        /// <returns>
        /// <b>true</b> if the card number is valid.
        /// </returns>
        public bool IsValid(String card)
        {
            // check card number length
            if ((card == null) || (card.Length < 13) || (card.Length > 19))
            {
                return false;
            }

            // check if the card is a valid credit card number
            if (!LuhnCheck(card))
            {
                return false;
            }

            // validate card with credit card type validator
            if (CardType != null)
            {
                return ValidateCard(card);
            }
            else
            {
                throw new ArgumentException("Property CardType cannot be null.");
            }
        }

        /// <summary>
        /// Validates card number with the specified <see cref="CardType"/> validator.
        /// </summary>
        /// <param name="cardNumber">
        /// Credit card number to validate.
        /// </param>
        /// <returns>
        /// <b>true</b> if credit card number is a valid number of credit card type specified.
        /// </returns>
        private bool ValidateCard(string cardNumber)
        {
            String card = cardNumber == null ? null : cardNumber.Trim();

            if (card == null)
            {
                return false;
            }

            return CardType.Matches(card);
        }

        /// <summary>
        /// Checks for a valid credit card number.
        /// </summary>
        /// <param name="cardNumber">
        /// Credit Card Number.
        /// </param>
        /// <returns>
        /// <b>true</b> if the card number passes the LuhnCheck.
        /// </returns>
        private bool LuhnCheck(string cardNumber)
        {
            // number must be validated as 0..9 numeric first!!
            int digits = cardNumber.Length;
            int oddOrEven = digits & 1;
            long sum = 0;
            for (int count = 0; count < digits; count++)
            {
                int digit;
                try
                {
                    digit = Int32.Parse(cardNumber[count] + "");
                }
                catch (FormatException)
                {
                    return false;
                }

                if (((count & 1) ^ oddOrEven) == 0)
                { // not
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }
                sum += digit;
            }

            return (sum == 0) ? false : (sum % 10 == 0);
        }

        #endregion

        #region Data members

        private ICreditCardType m_cardType;

        #endregion
    }

    #region CreditCardType classes

    /// <summary>
    /// CreditCardType interface defines how validation is performed
    /// for one type/brand of credit card.
    /// </summary>
    public interface ICreditCardType
    {
        /// <summary>
        /// Returns true if the card number matches this type of
        /// credit card.
        /// </summary>
        /// <param name="card">
        /// The card number, never null.
        /// </param>
        /// <returns>
        /// <b>true</b> if the number matches.
        /// </returns>
        bool Matches(String card);
    }

    /// <summary>
    /// Visa credit card type validation support.
    /// </summary>
    public class Visa : ICreditCardType
    {
        private static readonly String PREFIX = "4";

        /// <summary>
        /// Indicates, wheter the given credit card number matches a visa number.
        /// </summary>
        public bool Matches(String card)
        {
            return (card.Substring(0, 1).Equals(PREFIX) && (card.Length == 13 || card.Length == 16));
        }
    }

    /// <summary>
    /// American Express credit card type validation support.
    /// </summary>
    public class Amex : ICreditCardType
    {
        private static readonly String PREFIX = "34,37,";

        /// <summary>
        /// Indicates, wheter the given credit card number matches an amex number.
        /// </summary>
        public bool Matches(String card)
        {
            String prefix2 = card.Substring(0, 2) + ",";
            return ((PREFIX.IndexOf(prefix2) != -1) && (card.Length == 15));
        }
    }

    /// <summary>
    /// Discover credit card type validation support.
    /// </summary>
    public class Discover : ICreditCardType
    {
        private static readonly String PREFIX = "6011";

        /// <summary>
        /// Indicates, wheter the given credit card number matches a discover number.
        /// </summary>
        public bool Matches(String card)
        {
            return (card.Substring(0, 4).Equals(PREFIX) && (card.Length == 16));
        }
    }

    /// <summary>
    /// Mastercard credit card type validation support.
    /// </summary>
    public class Mastercard : ICreditCardType
    {
        private static readonly String PREFIX = "51,52,53,54,55,";

        /// <summary>
        /// Indicates, wheter the given credit card number matches a mastercard number.
        /// </summary>
        public bool Matches(String card)
        {
            String prefix2 = card.Substring(0, 2) + ",";
            return ((PREFIX.IndexOf(prefix2) != -1) && (card.Length == 16));
        }
    }

    #endregion
}
