   M S B A M L   `   `   `  ���� I  ESimpleLoginWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:SimpleLoginWPFSimpleLoginWPF      SimpleLoginWPF.Login   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     .localclr-namespace:SimpleLoginWPF       � Title$  Login��$	��720��$
��1280��$��Georgia)�.���  � ColumnDefinitions �� $��* ��� $
��Auto ��� $
��Auto ��� $��* � � RowDefinitions �� $��* ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��1��$��1��$��5��$I��Cpack://application:,,,/SimpleLoginWPF;component/Assets/MAINLogo.png��$	��280��$	��280��$��Center=�$��Center=�$��	20,0,80,0q��� $��2��$��3��$��Center=�$��0,0,10,0q�.+��� $Z��Tpack://application:,,,/SimpleLoginWPF;component/Assets/aliflogo-removebg-preview.png��$	��120��$	��100��$��Center=�$��0,0,0,20q��� $��Login to your account��$��22(�$
��Bold"�$��Center=�$��0,0,0,0q��� $.��(Welcome back! Please enter your details.��$��15(�$��
UltraLight"� J� Opacity$	 0.5Q�$��Center=�$��0,0,0,30q��� $��Name*��$��16(�$��Italic$�$	 0.5Q�$��0,0,0,5q��� -    ��Name$ UsernameTextBox��$��Username��$��30��$��Center=�$��15(�$��1q�
�������$��5q�
�������$��0,0,0,20q�.������  �� 
TargetType" ��.���  �� Property � �� �� Value ��  �� 
TargetType" ��.R��� #��z��#��z��#��z��	 �� CornerRadius$	 5��._��� 
 ��Name$
 PART_ContentHost��$��0q�.���� $��	Password*��$��16(�$��Italic$�$	 0.5Q�$��0,0,0,5q�G� -    G�Name$ PasswordBox��$��30��$��15(�$��0,0,0,30q�$��1q�
�������$��5q�
����������� " G�.���  � �� �� " G�.R��� #��z��#��z��#��z��$	 5��._��� $
 PART_ContentHost��$��0q��� -    ��Name$ LoginButton��
���  ��
�������$��Login��$��40��$��16(�$
��Bold"�$��Center=�$��0,0,0,20q����� " ��.���  � �� �� " ��.R��� #��z��#��z��#��z��$	 5��._��� $��Center=�$��Center=���  � ��
 �  ����  � ��
 ����� �� Triggers P�  P� Property �@J�IsMouseOver P� Value$
 True��. ���  � �� �#B22222