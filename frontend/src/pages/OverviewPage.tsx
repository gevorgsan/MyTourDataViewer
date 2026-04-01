import './OverviewPage.scss';

export function OverviewPage() {
  return (
    <div className="overview-container">
      <div className="layout">
        {/* Left panel: system info */}
        <aside className="api-list">
          <h3>About This App</h3>
          <p className="system-info">
            This app allows users to view MyTour requests and booking history.
          </p>
        </aside>

        {/* Right panel: placeholder (API selection not yet implemented) */}
        <section className="data-panel">
          <p className="hint">&#8592; Select an API from the list to view its data.</p>
        </section>
      </div>
    </div>
  );
}
