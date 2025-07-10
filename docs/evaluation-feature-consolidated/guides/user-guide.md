# User Guide

## Getting Started

The Evaluation feature helps you systematically test and analyze AI model performance. Access it from the main navigation menu by clicking on "Evaluate".

## Creating an Evaluation

### Step 1: Choose Your Workflow

Select one of three evaluation workflows:

1. **Test Model** - Run your model against a dataset in real-time
2. **Evaluate Responses** - Score pre-generated model outputs
3. **Import Results** - Bring in evaluation data from external sources

### Step 2: Select Evaluation Type

Choose the type of evaluation based on your model:
- **Classification** - For models that categorize inputs
- **Generation** - For models that generate text/content
- **Custom** - Define your own evaluation criteria

### Step 3: Configure Your Model

For **Test Model** workflow:
- Select your model from the dropdown
- Configure model parameters (temperature, max tokens, etc.)
- Set any model-specific options

For **Evaluate Responses** workflow:
- This step shows a summary since responses are pre-generated

For **Import Results** workflow:
- Specify the format of your imported data

### Step 4: Upload Your Dataset

Supported formats:
- **JSONL files** - One JSON object per line
- **Image folders** - For vision models
- **CSV files** - Structured tabular data

Dataset requirements:
- Maximum 1,000 items (current limit)
- Each item must have required fields based on evaluation type
- UTF-8 encoding for text files

### Step 5: Select Metrics

Choose evaluation metrics relevant to your use case:
- **Accuracy** - Percentage of correct predictions
- **Precision/Recall** - For classification tasks
- **BLEU Score** - For text generation
- **Custom Metrics** - Define your own

### Step 6: Review and Launch

Review all your selections:
- Verify dataset was loaded correctly
- Check selected metrics
- Confirm model configuration
- Click "Launch Evaluation" to start

## Managing Evaluations

### View Options

Toggle between two view modes:
- **Card View** - Visual cards with preview information
- **List View** - Compact table format

### Filtering and Search

- Use the search box to find evaluations by name
- Filter by status: All, Running, Completed, Failed
- Sort by date, name, or score

### Bulk Operations

1. Select multiple evaluations using checkboxes
2. Use the action bar that appears:
   - **Delete** - Remove selected evaluations
   - **Export** - Export data to JSON/CSV
   - **Compare** - Launch comparison view

## Viewing Insights

### Individual Evaluation Analysis

Click on any evaluation to view detailed insights:

1. **Overview Section**
   - Average score and key metrics
   - Dataset and model information
   - Evaluation duration and status

2. **Score Distribution**
   - Histogram showing score ranges
   - Statistical measures (mean, median, std dev)
   - Percentile information

3. **Detailed Results**
   - Table with individual item scores
   - Sort by any column
   - Search within results
   - Export filtered results

4. **Performance Analysis**
   - Metrics breakdown by criteria
   - Identified patterns
   - Recommendations for improvement

### Printing Reports

Click the "Print" button to generate a printer-friendly report including:
- Summary statistics
- Key visualizations
- Top/bottom performing items
- Evaluation configuration

## Comparing Evaluations

### Launching Comparison

1. Select 2-4 evaluations from the list
2. Click "Compare Selected" in the action bar
3. Comparison view opens automatically

### Comparison Features

- **Side-by-side Metrics** - See all evaluations together
- **Relative Performance** - Percentage differences
- **Trend Analysis** - Performance over time
- **Statistical Tests** - Significance indicators

### Understanding Comparisons

- Green arrows indicate improvement
- Red arrows indicate degradation
- Gray means no significant change
- Confidence intervals show reliability

## Exporting Data

### Export Formats

1. **JSON Export**
   - Complete evaluation data
   - Preserves all metadata
   - Suitable for programmatic use

2. **CSV Export**
   - Tabular format
   - Opens in Excel/spreadsheet apps
   - Simplified structure

### Export Options

- **Summary Only** - High-level metrics
- **Include Details** - Individual results
- **Custom Selection** - Choose specific fields

## Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| New Evaluation | Ctrl+N |
| Search | Ctrl+F |
| Select All | Ctrl+A |
| Delete Selected | Delete |
| Toggle View | Ctrl+Tab |
| Open Insights | Enter |
| Close Dialog | Esc |

## Accessibility Features

- **Screen Reader Support** - All elements properly labeled
- **Keyboard Navigation** - Full keyboard access
- **High Contrast** - Adapts to Windows theme
- **Focus Indicators** - Clear visual focus states

## Tips and Best Practices

1. **Naming Evaluations**
   - Use descriptive names
   - Include date or version
   - Mention key parameters

2. **Dataset Preparation**
   - Validate JSON before uploading
   - Use consistent schema
   - Include ground truth labels

3. **Comparing Results**
   - Compare similar evaluation types
   - Use same metrics for fair comparison
   - Consider dataset differences

4. **Performance**
   - Keep evaluations under 1,000 items
   - Archive old evaluations
   - Export before deleting

## Troubleshooting

### Common Issues

**Upload Fails**
- Check file format (JSONL, not JSON)
- Verify UTF-8 encoding
- Ensure required fields present

**Evaluation Stuck**
- Check status in list view
- Try refreshing the page
- Check for error messages

**Charts Not Loading**
- Ensure evaluation has completed
- Check if data exists
- Try different view mode

### Getting Help

- Hover over (?) icons for tooltips
- Check validation messages
- Review error details in red bars