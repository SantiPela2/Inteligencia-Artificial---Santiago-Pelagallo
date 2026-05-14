using System;
using TreeEditor;
using UnityEngine;

public class WeightedNode : ITreeNode
{
    private (float weight, Action action)[] options;

    // Recibe una lista de opciones con su "peso" (probabilidad) y la acción a ejecutar
    public WeightedNode((float weight, Action action)[] options)
    {
        this.options = options;
    }

    public void Execute()
    {
        float totalWeight = 0f;
        foreach (var option in options)
        {
            totalWeight += option.weight;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var option in options)
        {
            currentWeight += option.weight;
            if (randomValue <= currentWeight)
            {
                option.action(); // Ejecuta la acción ganadora
                return;
            }
        }
    }
}