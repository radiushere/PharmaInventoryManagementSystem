   M S B A M L   `   `   `  ����   SimpleLoginWPF    SimpleLoginWPF.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Inventory Management��$	��720��$
��1280�����#F8FAFC$��Segoe UI)� � 	Resources    SidebarButtonStyle �� 
TargetType �� Property �� Value3  -System.Windows.Media.Effects.DropShadowEffect   
BlurRadius   Opacity �� 
TargetType �� Padding	 �� CornerRadius
 ^� ToolTip �� Orientation  AddMoreButton  FilterButton  SelectedSidebarButtonStyle �� BasedOn&   System.Windows.Controls.DataGrid  StatsBoxStyle  DetailsBoxesText  DataGridMain=  7System.Windows.Controls.Primitives.DataGridColumnHeader)  #System.Windows.Controls.DataGridRow �� Triggers P� Property P� Value   Color  SearchBoxStyle��%�
  &	        &	 Y    &	 �    &	     7   '  x    &	 )    &	 �    &	 �    &	 �	    �� " ��.���  � ��
 ���� ��  � �� �#444��  � ��$ 16(���  � ��$ 10q���  � ��$ Stretch=���  � ��$ 0q���  �@�Cursor$
 Hand����  �@J�Effect   $ 8Q�$	 0.3Q���  � �� �� " ��.R��� #��z��$ 10q�$	 6��#
 z��._��� $ 
Horizontal=�.+��� $��5,0,0,0q��� " ��.���  � ��
 ���� ��  � �� �#444��  � ��$ 16(���  � ��$ 10q���  � ��$ Stretch=���  � ��$ 0q���  �@�Cursor$
 Hand����  �@J�Effect   $ 10Q�$	 0.3Q���  � �� �� " ��.R��� #��z��$ 10q�$	 6��#
 z��._��� $ 
Horizontal=�.+��� $��5,0,0,0q��� " ��.���  � ��
 ���� ��  � �� �#444��  � ��$ 16(���  � ��$ 10q���  � ��$ Stretch=���  � ��$ 0q���  �@�Cursor$
 Hand����  �@J�Effect   $ 10Q�$	 0.3Q���  � �� �� " ��.R��� #��z��$ 10q�$	 6��#
 z��._��� $ 
Horizontal=�.+��� $��5,0,0,0q�$��70���� " ��8   .���  � �� �#FFD9D9��  � �� �#B30000��  � ��$
 Bold"��� "  .���  �@ RowBackground �#FFFFFF��   �@ AlternatingRowBackground �#F2F2F2��  � ��
 �������  � ��$ 1q���  � ��
 �   ��� " ��.���  � �� �#FFF8E1��  �@��CornerRadius$ 6����  �@��Padding$ 10q���  � ��$ 5q���  �@J�Effect   $ 6Q�$	 0.2Q��� " ��.���  � ��
 ���� ��  � �� �#444��  � ��$ 12(���  �@��Padding$ 0q���  � ��$ Stretch=���  �@J�Effect   $ 6Q�$	 0.1Q��� "  .���  � �� �#FFF8F8��  �@ RowBackground
 �������   �@ AlternatingRowBackground �#FFEEEE��  � �� �#B30000��  � ��$ 1q���  � ��
 �   ���   �@ HorizontalGridLinesBrush �#FFAAAA��  �@ VerticalGridLinesBrush �#FFAAAA��  �@ ColumnHeaderStyle �� "  .���  � �� �#B30000��  � ��
 �������  � ��$
 Bold"���  � ��$ 8q���  � ��$ 0,0,0,1q���  � �� �#800000��  �@ RowStyle �� "  .���  �@�Cursor$
 Hand����  � ��
 �������  � ��
 ���� ��  � ��$ 0q� P�  �@J�IsMouseOver$
 True��. ���  � �� �#FFD9D9��  �@J�Effect   $ 6Q�$	 0.3Q�$ #B30000��P�  �@ 
IsSelected$
 True��. ���  � �� �#FFBBBB��  � ��
 �   ��� " ��.���  � ��
 �������  � ��$ 1q���  � ��
 �   ���  �@J�Effect   $ 10Q�$	 0.3Q�.��� .+��� 
�������$	��240��$
��Left=�.+��� $+��%/Assets/aliflogo-removebg-preview.png��$	��140��$	��100��$��20q��� -   #��[  .���� $ 
Horizontal=�.+��� $��/Assets/Home.png��$��22��$��22��$��5q��� $��	Dashboard���� -   #��[ .���� $ 
Horizontal=�.+��� $$��/Assets/Inventory_Selected.png��$��22��$��22��$��5q��� $��	Inventory���� -   #��[  .���� $ 
Horizontal=�.+��� $��/Assets/Report.png��$��22��$��22��$��5q��� $��Reports���� -   #��[  .���� $ 
Horizontal=�.+��� $��/Assets/Suppliers.png��$��22��$��22��$��5q��� $��	Suppliers���� -   #��[  .���� $ 
Horizontal=�.+��� $��/Assets/Order.png��$��22��$��22��$��5q��� $��Orders���� -   #��[  .���� $ 
Horizontal=�.+��� $��/Assets/Manage Store.png��$��22��$��22��$��5q��� $��Manage Store���� -   #��[  
���  ��.���� $ 
Horizontal=�.+��� $��/Assets/Logout.png��$��22��$��22��$��5q��� $��Logout��� $��250,20,20,20q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* �.+�]� $
��True��$��0��.+��� -    ��Name$ 	SearchBox��#��[ $��Search Medicine...��$	��350��$��40��$��18(�
�������$��Center=�$��10q��� #��[ $��Add Products��$	��120��$��40��
����������	#FFBB1A1A$��10q�$
��Bold"��� -	   #��[ $��
   Filters��$	��100��$��40��
���   ����#EAEAEA$��10,10,50,10q��� $
��👤��$��40��$��40��
������ $��200,5,-100,5q�� $��1��
�������$��	0,10,0,10q� � ColumnDefinitions �� $��* ��� $��* ��� $��* ��� $��* �.+��� #��[ $��0�����#FFF5CC$ 10q�._��� .+��� $��
Categories��$
��Bold"����#B38F00�� $��14��$��18(�$
��Bold"��� #��[ $��1�����#E5F7FF$ 10q�._��� .+��� $��Total Products��$
��Bold"����#0088CC�� $	��868��$��18(�$
��Bold"��� #��[ $��2�����#E3F2FD$ 10q�._��� .+��� $��Top Selling��$
��Bold"����#007BFF�� $��5��$��18(�$
��Bold"��� #��[ $��3�����#FFF0E0$ 10q�._��� .+��� $��
Low Stocks��$
��Bold"����#CC5500�� $��12��$��18(�$
��Bold"�  -
     Name$ MedicinesDataGrid��#��[ $��2��   AutoGenerateColumns$ False��$��0,10,0,0q�
�������$��1q�   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ ID��   Width5  /System.Windows.Controls.DataGridLengthConverter$ 50 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *ID+  $
 Name��$	 150  �� *Name+  $ Quantity��$	 100  �� *
Quantity+  $ Price��$	 100  �� *Price+  $ Expiry Date��$	 120  �� *
ExpiryDate+  $ 	Batch No.��$	 100  �� *BatchNumber+  $ Description��$	 200  �� *Description+