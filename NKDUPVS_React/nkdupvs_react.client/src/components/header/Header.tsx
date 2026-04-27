import ktulogo from '../../KTU_logo.png';
import './Header.scss';
import { NavLink } from 'react-router-dom';

/**
 * Page header. React component.
 * @returns Component HTML.
 */

function Header() {
    let html =        
        <header>
			<div className="header-content">
				<NavLink to="/home"><img className="header-logo" src={ktulogo} alt="ktu logo"/></NavLink>
				<h2 className='header-title'>NAUJOS KARTOS DĖSTYTOJŲ UGDYMO PROCESO VALDYMO SISTEMA</h2>
			</div>
        </header>

    return html;
}

// Export component
export default Header;