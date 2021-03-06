# DataKeeper

DataKeeper is a helpful model for your small C# projects. It will be very convenient if for your work it's enough to save data between threads/forms/windows and use data triggers.

  - Use C# actions to bind remove/update triggers
  - You can also choose when a trigger is called: before or after

Work with the model such as a Dictionary where keys are strings!

### Examples!
```csharp examples
Data model = Data.Instance;
model.BindUpdateField(
  "some_key",
  () => Console.WriteLine("value on some_key is updated")
);
model.Set("some_key", value: 42); // will set new value and print "value on some_key is updated"
// Set takes an object so you can put any value
```

More examples and API in [wiki].

[wiki]: <https://github.com/TezRomacH/DataKeeper/wiki>
