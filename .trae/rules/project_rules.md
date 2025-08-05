Unreal-GUI项目使用了CommunityToolkit.Mvvm，请在编写代码的时候，使用CommunityToolkit.Mvvm的命令绑定，而不是使用传统的事件绑定，例如：

```
<Button Content="添加" Command="{Binding AddCustomButtonCommand}" />
```

在ViewModel中，定义命令：
```
  [ObservableProperty]
    private string name;

    [ObservableProperty]
    private int age;

    [ObservableProperty]
    private bool isBusy;
```
而不是：
```
public string Name
{
    get => name;
    set => SetProperty(ref name, value);
}
```

···等等，类似的，请使用[INotifyPropertyChanged]和[RelayCommand]，而不是传统的事件绑定和委托。
