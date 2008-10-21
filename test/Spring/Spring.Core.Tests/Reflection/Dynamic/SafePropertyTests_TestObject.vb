Public Class VbTestObject
    Public ThisArg1 As Object
    Public ThisArg2 As Object
    Public ThisOptionalArg As Object
    Public ThisParamsArgs As Object()
    Public ThisLastPropertyValue As Object

    Property SimpleProperty() As String
        Get
            Return ThisLastPropertyValue
        End Get
        Set(ByVal value As String)
            ThisLastPropertyValue = value
        End Set
    End Property

    Property SimpleIndexer(ByVal arg1 As Integer) As String
        Get
            ThisArg1 = arg1
            Return ThisLastPropertyValue
        End Get
        Set(ByVal value As String)
            ThisArg1 = arg1
            ThisLastPropertyValue = value
        End Set
    End Property

    Default Property ComplexIndexer(ByVal arg1 As Double, ByVal arg2 As Object) As String
        Get
            ThisArg1 = arg1
            ThisArg2 = arg2
            Return ThisLastPropertyValue
        End Get
        Set(ByVal value As String)
            ThisArg1 = arg1
            ThisArg2 = arg2
            ThisLastPropertyValue = value
        End Set
    End Property

    Property PropertyWithParamsArgs(ByVal arg1 As String, ByVal ParamArray paramsArgs As Object()) As String
        Get
            ThisArg1 = arg1
            ThisParamsArgs = paramsArgs
            Return ThisLastPropertyValue
        End Get
        Set(ByVal value As String)
            ThisArg1 = arg1
            ThisParamsArgs = paramsArgs
            ThisLastPropertyValue = value
        End Set
    End Property


    Property PropertyWithOptionalArg(ByVal arg1 As String, Optional ByVal optionalArg As Object = "Empty") As String
        Get
            ThisArg1 = arg1
            ThisOptionalArg = optionalArg
            Return ThisLastPropertyValue
        End Get
        Set(ByVal value As String)
            ThisArg1 = arg1
            ThisOptionalArg = optionalArg
            ThisLastPropertyValue = value
        End Set
    End Property

End Class