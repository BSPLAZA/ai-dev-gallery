# Known Issues and Future Work

## Current Build Issues

Both PRs have minor build failures that don't affect functionality:

### XML Spacing Violations
- Inconsistent indentation in XAML files
- Extra spaces in element declarations
- Can be fixed with auto-formatting

### Code Style Issues
- Missing copyright headers
- StyleCop warnings about documentation
- Line length violations
- Whitespace inconsistencies

These are formatting issues only - the code compiles and runs correctly locally.

## Technical Debt

### Accessibility Improvements Needed

While basic accessibility is implemented, these areas need enhancement:

1. **Dynamic Content Announcements**
   - List updates aren't announced to screen readers
   - Need better live region management
   - Status changes should be announced

2. **Focus Management**
   - After delete operations, focus should return to list
   - Modal dialogs need proper focus trapping
   - Tab order could be optimized in complex forms

3. **Chart Accessibility**
   - Current descriptions are basic
   - Need detailed data tables as alternatives
   - Keyboard navigation within charts incomplete

4. **Screen Reader Optimization**
   - Some repetitive announcements
   - Context could be clearer in places
   - Landmarks need better organization

### Performance Limitations

1. **Dataset Size**
   - Currently limited to 1,000 items
   - No streaming support for large datasets
   - Memory usage grows linearly

2. **Chart Rendering**
   - Basic WinUI controls used
   - No hardware acceleration
   - Refreshes could be optimized

3. **Search Implementation**
   - Currently does client-side filtering
   - No indexing for large datasets
   - Case-sensitive in some places

### Missing Features

1. **Backend Integration**
   - All data is currently mocked
   - No real API endpoints implemented
   - Authentication not integrated

2. **Advanced Analytics**
   - Only basic statistics available
   - No ML-specific metrics (F1, AUC, etc.)
   - Limited statistical tests

3. **Export Options**
   - No Excel export
   - Limited formatting options
   - No template support

4. **Collaboration**
   - No sharing functionality
   - No team features
   - No cloud sync

## Future Roadmap

### Phase 1: Production Readiness
1. Fix all build issues
2. Complete accessibility gaps
3. Implement real backend
4. Add comprehensive error handling
5. Performance optimization

### Phase 2: Enhanced Analytics
1. Advanced statistical metrics
2. ML-specific evaluations
3. Custom metric builder
4. Trend detection algorithms
5. Anomaly detection

### Phase 3: Enterprise Features
1. Team collaboration
2. Cloud storage integration
3. Advanced export options
4. Audit logging
5. Role-based access control

### Phase 4: Advanced Visualizations
1. Interactive 3D charts
2. Real-time streaming updates
3. Custom visualization builder
4. Dashboard designer
5. Report templates

## Migration Notes

When moving from mock to real implementation:

1. **Data Migration**
   - Existing local evaluations should be preserved
   - Provide upload tool for historical data
   - Maintain backward compatibility

2. **API Integration**
   - Start with read operations
   - Gradually add write operations
   - Implement offline support

3. **User Settings**
   - Migrate preferences
   - Preserve custom configurations
   - Handle schema changes gracefully

## Known Workarounds

### For Build Issues
- Ignore spacing warnings for now
- Focus on functionality over formatting
- Will be fixed in cleanup phase

### For Performance
- Keep evaluations under 500 items for best performance
- Use list view instead of card view for large sets
- Export and archive old evaluations

### For Missing Features
- Use export/import for sharing
- Calculate advanced metrics externally
- Use print feature for reports

## Bug Tracker

### High Priority
- [ ] Focus not returning after delete operation
- [ ] Memory leak in chart controls (suspected)
- [ ] Search doesn't handle special characters

### Medium Priority
- [ ] Tooltip positioning issues at screen edges
- [ ] Sort order not preserved after refresh
- [ ] Validation messages sometimes cut off

### Low Priority
- [ ] Animation stutters on low-end hardware
- [ ] Theme switching requires restart
- [ ] Keyboard shortcuts not customizable

## Contributing

When working on fixes:

1. **Check PR Status**
   - Public repo: PRs #5 and #6
   - Private repo: PRs #23 and #25
   - Ensure you're on the right branch

2. **Testing Requirements**
   - Test on Windows 11
   - Verify accessibility with Narrator
   - Check high contrast mode
   - Test with 100+ item datasets

3. **Code Standards**
   - Follow existing patterns
   - Add XML documentation
   - Include unit tests
   - Update relevant docs

## Support

For questions or issues:
- Check existing documentation first
- Review PR comments for context
- Test in isolation when possible
- Document any new findings