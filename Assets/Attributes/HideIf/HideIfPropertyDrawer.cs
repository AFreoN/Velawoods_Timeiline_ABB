using Exceptions;
using UnityEditor;
using UnityEngine;
using Utilities;

[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfPropertyDrawer : PropertyDrawer
{
    // Reference to the attribute on the property.
    HideIfAttribute hideIf;

    // Field that is being compared.
    SerializedProperty comparedField;

    // Height of the property.
    private float propertyHeight;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!HideMe(property) && hideIf.disablingType == DisablingType.DontDraw)
            return 0f;

        return base.GetPropertyHeight(property, label);
        //return propertyHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Set the global variables.
        hideIf = attribute as HideIfAttribute;
        comparedField = property.serializedObject.FindProperty(hideIf.comparedPropertyName);

        // Get the value of the compared field.
        object comparedFieldValue = comparedField.GetValue<object>();

        // References to the values as numeric types.
        NumericType numericComparedFieldValue = null;
        NumericType numericComparedValue = null;

        try
        {
            // Try to set the numeric types.
            numericComparedFieldValue = new NumericType(comparedFieldValue);
            numericComparedValue = new NumericType(hideIf.comparedValue);
        }
        catch (NumericTypeExpectedException)
        {
            // This place will only be reached if the type is not a numeric one. If the comparison type is not valid for the compared field type, log an error.
            if (hideIf.comparisonType != ComparisonType.Equals && hideIf.comparisonType != ComparisonType.NotEqual)
            {
                Debug.LogError("The only comparsion types available to type '" + comparedFieldValue.GetType() + "' are Equals and NotEqual. (On object '" + property.serializedObject.targetObject.name + "')");
                return;
            }
        }

        // Is the condition met? Should the field be drawn?
        bool conditionMet = true;

        // Compare the values to see if the condition is met.
        switch (hideIf.comparisonType)
        {
            case ComparisonType.Equals:
                if (comparedFieldValue.Equals(hideIf.comparedValue))
                    conditionMet = false;
                break;

            case ComparisonType.NotEqual:
                if (!comparedFieldValue.Equals(hideIf.comparedValue))
                    conditionMet = false;
                break;

            case ComparisonType.GreaterThan:
                if (numericComparedFieldValue > numericComparedValue)
                    conditionMet = false;
                break;

            case ComparisonType.SmallerThan:
                if (numericComparedFieldValue < numericComparedValue)
                    conditionMet = false;
                break;

            case ComparisonType.SmallerOrEqual:
                if (numericComparedFieldValue <= numericComparedValue)
                    conditionMet = false;
                break;

            case ComparisonType.GreaterOrEqual:
                if (numericComparedFieldValue >= numericComparedValue)
                    conditionMet = false;
                break;
        }

        // The height of the property should be defaulted to the default height.
        propertyHeight = base.GetPropertyHeight(property, label);

        // If the condition is met, simply draw the field. Else...
        if (conditionMet)
        {
            EditorGUI.PropertyField(position, property, label);
        }
        else
        {
            //...check if the disabling type is read only. If it is, draw it disabled, else, set the height to zero.
            if (hideIf.disablingType == DisablingType.ReadOnly)
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label);
                GUI.enabled = true;
            }
            else
            {
                propertyHeight = 0f;
            }
        }
    }

    private bool HideMe(SerializedProperty property)
    {
        hideIf = attribute as HideIfAttribute;
        // Replace propertyname to the value from the parameter
        string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, hideIf.comparedPropertyName) : hideIf.comparedPropertyName;

        comparedField = property.serializedObject.FindProperty(path);

        if (comparedField == null)
        {
            Debug.LogError("Cannot find property with name: " + path);
            return true;
        }

        // get the value & compare based on types
        switch (comparedField.type)
        { // Possible extend cases to support your own type
            case "bool":
                return !comparedField.boolValue.Equals(hideIf.comparedValue);
            case "Enum":
                return !comparedField.enumValueIndex.Equals((int)hideIf.comparedValue);
            default:
                Debug.LogError("Error: " + comparedField.type + " is not supported of " + path);
                return true;
        }
    }
}