# 🏨 THE REGENCY MAINTENANCE

Sistema di gestione manutenzione hotel — **WinUI 3 · C# · .NET 8 · Windows App SDK 1.5**

---

## 🇮🇹 Come aprire con Visual Studio 2022

### Prerequisiti (da installare una volta sola)

| Cosa | Dove scaricarlo |
|------|----------------|
| **Visual Studio 2022** (Community, Professional o Enterprise) | https://visualstudio.microsoft.com/it/downloads/ |
| Workload **"Sviluppo di applicazioni per desktop .NET"** | Selezionarlo durante l'installazione di VS (vedi sotto) |
| **Windows App SDK 1.5** (incluso automaticamente tramite NuGet) | Installato automaticamente al primo build |
| **Windows 10 versione 2004 o superiore** (build 19041+) | Requisito di sistema |

> **Come scegliere il workload corretto in Visual Studio Installer:**
> 1. Apri **Visual Studio Installer** (Start → "Visual Studio Installer")
> 2. Clicca **Modifica** accanto a Visual Studio 2022
> 3. Spunta **"Sviluppo di applicazioni per desktop .NET"**
> 4. Nella colonna destra spunta anche **"Windows App SDK C# Templates"** (o "SDK Windows App")
> 5. Clicca **Modifica / Installa**

---

### Passaggi per aprire il progetto

**Passo 1 — Scarica il repository**

Hai due opzioni:

- **Opzione A – Con Git** (consigliata):
  ```
  git clone https://github.com/zamfirioan72-eng/RegencyMaintenance.git
  ```
  oppure usa **Git → Clone Repository** direttamente dal menu di Visual Studio.

- **Opzione B – Download ZIP**:
  Vai su https://github.com/zamfirioan72-eng/RegencyMaintenance, clicca **Code → Download ZIP**, estrai la cartella.

---

**Passo 2 — Apri la soluzione**

Nella cartella scaricata trovi il file:
```
RegencyMaintenance.sln
```
**Fai doppio clic** su `RegencyMaintenance.sln` — Visual Studio 2022 si apre direttamente con il progetto caricato.

> In alternativa: Visual Studio → **File → Apri → Progetto/Soluzione** → seleziona `RegencyMaintenance.sln`

---

**Passo 3 — Ripristina i pacchetti NuGet**

Alla prima apertura Visual Studio scarica automaticamente i pacchetti NuGet necessari (`Microsoft.WindowsAppSDK` e `Microsoft.Windows.SDK.BuildTools`).

Se non avviene in automatico:
- Clicca con il tasto destro sulla **Soluzione** nel Solution Explorer
- Seleziona **"Ripristina pacchetti NuGet"**

---

**Passo 4 — Seleziona la piattaforma**

Nella barra degli strumenti in alto, imposta:
- Configurazione: **Debug**
- Piattaforma: **x64** (o x86 per sistemi a 32 bit)
- Progetto di avvio: **RegencyMaintenance**

---

**Passo 5 — Avvia l'applicazione**

Premi **F5** (oppure il pulsante ▶ **Avvia** nella barra degli strumenti).

L'applicazione si avvierà mostrando la schermata di login.

---

### ⚠️ Problemi comuni e soluzioni

| Problema | Soluzione |
|----------|-----------|
| *"Could not find Windows App SDK"* | Vai su **Strumenti → NuGet Package Manager → Gestisci pacchetti NuGet per la soluzione** e ripristina i pacchetti |
| *"NETSDK1138: The target framework is not supported"* | Installa **.NET 8 SDK** da https://dotnet.microsoft.com/download/dotnet/8.0 |
| *"No usable version of libssl"* / errori SSL | Aggiorna Visual Studio 2022 all'ultima versione |
| *"Windows App SDK requires Windows 10 2004"* | Assicurati di usare Windows 10 build 19041 o Windows 11 |
| L'app si avvia ma crasha subito | Controlla che la **piattaforma** sia x64 (non ARM64 se il tuo PC è x64) |
| Errore di build su XAML | Esegui **Build → Pulisci soluzione**, poi **Build → Compila soluzione** |

---

### Struttura del progetto

```
RegencyMaintenance.sln              ← Apri questo con doppio clic
RegencyMaintenance/
├── RegencyMaintenance.csproj       ← .NET 8 · WinUI 3 · Windows App SDK 1.5
├── App.xaml / App.xaml.cs          ← Punto di ingresso dell'app
├── MainWindow.xaml                 ← Finestra principale (contiene il Frame di navigazione)
├── Models/                         ← Classi dati (Intervention, AppData, ecc.)
├── Services/                       ← DataService, BackupService, ReportService
├── Helpers/                        ← DateHelper (utility date italiane)
└── Views/
    ├── LoginPage.xaml              ← Schermata di login operatore
    ├── MainPage.xaml               ← Griglia stanze + barra comandi
    ├── RoomDialog.xaml             ← Finestra stanza (note + filtri + batterie)
    ├── AllInterventionsDialog.xaml ← Lista tutti gli interventi
    ├── PeriodFilterDialog.xaml     ← Filtro per periodo
    ├── BackupDialog.xaml           ← Gestione backup ZIP
    └── ReportPreviewDialog.xaml    ← Anteprima report giornaliero
```

---

### Dati salvati

L'applicazione salva i dati nel file `manutenzione_data.json` nella stessa cartella dell'eseguibile:
```
bin\x64\Debug\net8.0-windows10.0.19041.0\manutenzione_data.json
```
I backup ZIP vengono salvati nella sottocartella `backups\`.
I report TXT vengono salvati nella sottocartella `reports\`.

---

## 🇬🇧 How to open with Visual Studio 2022 (English)

### Prerequisites

1. **Visual Studio 2022** (any edition) — https://visualstudio.microsoft.com/downloads/
2. During installation, select the **".NET Desktop Development"** workload and include **"Windows App SDK C# Templates"**
3. **.NET 8 SDK** (usually included with VS 2022 17.8+)
4. **Windows 10 version 2004** (build 19041) or later

### Quick Start

```bash
git clone https://github.com/zamfirioan72-eng/RegencyMaintenance.git
```

1. Double-click **`RegencyMaintenance.sln`** to open in Visual Studio 2022
2. Wait for NuGet packages to restore automatically
3. Set platform to **x64** in the toolbar
4. Press **F5** to build and run

The app opens at the operator login screen. Enter any name and press **ENTRA** to start.

---

## Requisiti di sistema / System Requirements

- Windows 10 versione 2004 (build 19041) o superiore / or later
- Windows 11 (tutte le versioni / all versions)
- .NET 8 Runtime (incluso nel build / included in build output)
- Visual Studio 2022 versione 17.0 o superiore / version 17.0 or later