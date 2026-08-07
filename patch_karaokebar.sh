sed -i 's/            var tag = GetTag();/            return;/g' src/ui/Controls/KaraokeBarControl.cs
sed -i 's/            int totalCentiseconds = (int)System.Math.Round(_vm.SelectedSubtitle.Duration.TotalMilliseconds \/ 10.0);//g' src/ui/Controls/KaraokeBarControl.cs
sed -i 's/            text = "{" + tag + totalCentiseconds + "}" + text;//g' src/ui/Controls/KaraokeBarControl.cs
sed -i 's/            ActualizarTexto(text);//g' src/ui/Controls/KaraokeBarControl.cs
sed -i 's/            return;//g' src/ui/Controls/KaraokeBarControl.cs
