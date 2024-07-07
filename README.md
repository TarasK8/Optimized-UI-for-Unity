# Optimized UI
A package for Unity that is designed to greatly simplify user interface development.

> [!WARNING]
> The package is currently in an early stage of development, so some methods and concepts may change over time. Use at your own risk

## Features
- State Machine: This is an animator replacement focused on switching between visual states of UI elements, such as buttons.
- Flex components: an analogue of CSS Flex in web design. Automatically places elements in a container, you can specify Grow, Shrink, Basis for each element

## State Machine Usage
### Configure State Machine Component
1. Add the `State Machine` component to the interface element.
2. In the State Machine component inspector, add and configure the needed transitions. You can choose from the built-in ones or create your own, as described below.
   
   ![image](https://github.com/TarasK8/Optimized-UI-for-Unity/assets/108939631/316553ec-1bbd-45dd-9f1b-52312d8b7bb3)

3. Add the needed states and set their values, for example, for a button it can be "normal", "hover", and "pressed".

   ![image](https://github.com/TarasK8/Optimized-UI-for-Unity/assets/108939631/f6db9260-60d8-4b0f-91ef-c923e62f0782)


### Application of the State Machine component
- If you want to make a button or slider, then you can use a special Button or Slider component in which you can use the State Machine to 
