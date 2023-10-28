using System.Windows;
using System.Windows.Controls;

namespace SyncIMEStatus
{
    public class CustomListBox : ListBox
    {
        public bool HasSelection
        {
            get { return (bool)GetValue(HasSelectionProperty); }
            set { SetValue(HasSelectionProperty, value); }
        }

        public static readonly DependencyProperty HasSelectionProperty =
            DependencyProperty.Register("HasSelection", typeof(bool), typeof(CustomListBox),
                                        new PropertyMetadata(false));

        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent LostSelectionEvent = EventManager.RegisterRoutedEvent(
            name: "LostSelection",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(CustomListBox));

        // Provide CLR accessors for assigning an event handler.
        public event RoutedEventHandler LostSelection
        {
            add { AddHandler(LostSelectionEvent, value); }
            remove { RemoveHandler(LostSelectionEvent, value); }
        }

        void RaiseLostSelectionEvent()
        {
            RoutedEventArgs routedEventArgs = new RoutedEventArgs(
                routedEvent: LostSelectionEvent);

            RaiseEvent(routedEventArgs);
        }

        public CustomListBox()
        {
            SelectionChanged += (s, e) =>
            {
                if (SelectedItem == null)
                {
                    if (Items.Count == 0)
                    {
                        HasSelection = false;
                        RaiseLostSelectionEvent();
                    }
                    else
                    {
                        SelectedItem = Items.CurrentItem;
                    }
                }
                else
                {
                    HasSelection = true;
                }
            };
        }
    }

}
