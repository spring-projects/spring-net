// JScript File

// This function calls the Contact Web service method 
// passing simple type parameters and the callback function  
function GetEmails(prefix, count)
{
    ContactWebService.GetEmails(prefix, count, GetEmailsOnSucceeded);
}

// This is the callback function invoked if the Web service succeeded.
// It accepts the result object as a parameter.
function GetEmailsOnSucceeded(result, eventArgs)
{
    // Page element to display feedback.
    var ResultSpan = document.getElementById("ResultSpan");
    ResultSpan.innerHTML = '';
    var i = 0;
    for(email in result)
    {
        ResultSpan.innerHTML += (result[i] + '<br />');
        i++;
    }
}