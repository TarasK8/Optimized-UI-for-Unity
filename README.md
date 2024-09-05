# Optimized UI
A package for Unity that is designed to greatly simplify user interface development.

> [!WARNING]
> The package is currently in an early stage of development, so some methods and concepts may change over time. Use at your own risk

## Features
- State Machine: This is an animator replacement focused on switching between visual states of UI elements, such as buttons.
- Flex components: an analogue of CSS Flex in web design. Automatically places elements in a container, you can specify Grow, Shrink, Basis for each element

## State Machine Usage
### 1. Configure State Machine
1. Add the `State Machine` component to the interface element.
2. In the inspector, add and configure the needed transitions. You can choose from the built-in ones or create your own, as described below.
   
   ![image](https://github.com/TarasK8/Optimized-UI-for-Unity/assets/108939631/316553ec-1bbd-45dd-9f1b-52312d8b7bb3)

3. Add the needed states and set their values, for example, for a button it can be "normal", "hover", and "pressed".

   ![image](https://github.com/TarasK8/Optimized-UI-for-Unity/assets/108939631/f6db9260-60d8-4b0f-91ef-c923e62f0782)

### 2. Use State Machine
- If you want to make a simple element like a button or slider, you can use a special components (`Button`, `Slider`, `Scrollbar` or `Selectable`), in this package they are modified to use `State Machine` for transitions. All available components can be found in the `Optimized UI` tab in the `Add Component` menu.

  ![image](https://github.com/TarasK8/Optimized-UI-for-Unity/assets/108939631/10f067f5-8fcb-4ac6-9e5b-de2af0966cc1)

  
