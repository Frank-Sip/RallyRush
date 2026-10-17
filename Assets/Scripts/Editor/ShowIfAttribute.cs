using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowIfAttribute : PropertyAttribute
{
    public string boolName;

    public ShowIfAttribute(string boolName)
    {
        this.boolName = boolName;
    }
}